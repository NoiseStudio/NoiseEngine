﻿using NoiseEngine.Mathematics;
using NoiseEngine.Mathematics.Advanced;
using System;
using System.Diagnostics;

namespace NoiseEngine.Physics.Collision.Mesh;

internal static class Epa {

    public const int MaxIterations = 64;
    private const float Epsilon = 0.0001f;

    public static EpaResult Process(
        in Simplex3D simplex, in Isometry3<float> pos12, in ConvexHullId hullA, in float3 scaleA,
        in ConvexHullId hullB, in float3 scaleB, ReadOnlySpan<float3> verticesA, ReadOnlySpan<float3> verticesB
    ) {
        Span<SupportPoint> polytopeVertices = stackalloc SupportPoint[MaxIterations];
        simplex.CopyTo(polytopeVertices);
        Span<PolytopeFace> polytopeFaces = stackalloc PolytopeFace[MaxIterations * 3];
        Span<int> polytopeFaceIds = stackalloc int[MaxIterations * 3];

        int verticesCount;
        int facesCount;
        if (simplex.Dimensions == 3) {
            float3 dp1 = simplex.B.Value - simplex.A.Value;
            float3 dp2 = simplex.C.Value - simplex.A.Value;
            float3 dp3 = simplex.D.Value - simplex.A.Value;
            if (dp1.Cross(dp2).Dot(dp3) > 0) {
                polytopeVertices[1] = simplex.C;
                polytopeVertices[2] = simplex.B;
            }

            polytopeFaces[0] = new PolytopeFace(polytopeVertices, new int3(0, 1, 2), new int3(3, 1, 2));
            polytopeFaces[1] = new PolytopeFace(polytopeVertices, new int3(1, 3, 2), new int3(3, 2, 1));
            polytopeFaces[2] = new PolytopeFace(polytopeVertices, new int3(0, 2, 3), new int3(0, 1, 3));
            polytopeFaces[3] = new PolytopeFace(polytopeVertices, new int3(0, 3, 1), new int3(2, 1, 0));

            verticesCount = 4;
            facesCount = 4;
        } else {
            Debug.Assert(simplex.Dimensions == 1 || simplex.Dimensions == 2);
            if (simplex.Dimensions == 1) {
                float3 direction = Orthonormal.SubspaceBasisOnce(simplex.B.Value - simplex.A.Value);
                polytopeVertices[2] = Minkowski.FindSupportPoint(
                    direction, pos12, hullA, scaleA, hullB, scaleB, verticesA, verticesB
                );
            }

            polytopeFaces[0] = new PolytopeFace(polytopeVertices, new int3(0, 1, 2), new int3(1, 1, 1));
            polytopeFaces[1] = new PolytopeFace(polytopeVertices, new int3(0, 2, 1), new int3(0, 0, 0));

            polytopeFaceIds[1] = 1;

            verticesCount = 3;
            facesCount = 2;
        }

        Polytope3D polytope = new Polytope3D(polytopeVertices, verticesCount, polytopeFaces, facesCount, polytopeFaceIds, facesCount);
        int bestFace = 0;
        float maxDistance = float.MaxValue;

        int i = 0;
        while (true) {
            (_, int faceId) = polytope.ClosestFaceToOrigin();
            ref PolytopeFace face = ref polytopeFaces[faceId];
            //if (face.IsDeleted)
            //    continue;

            SupportPoint supportPoint = Minkowski.FindSupportPoint(
                face.Normal, pos12, hullA, scaleA, hullB, scaleB, verticesA, verticesB
            );

            if (supportPoint.Value.Dot(face.Normal) - face.Distance <= Epsilon || i++ == MaxIterations) {
                //face = ref polytopeFaces[bestFace];
                //polytope.CheckTopology();
                return ComputeResult(polytope, face);
            }

            polytope.Add(ref face, supportPoint, faceId);
        }

        //ref PolytopeFace faceResult = ref polytopeFaces[bestFace];
        //return new EpaResult(ComputeResult(polytope, faceResult), faceResult.Normal, faceResult.Distance);
    }

    private static EpaResult ComputeResult(in Polytope3D polytope, in PolytopeFace face) {
        Span<SupportPoint> vertices = polytope.Vertices;
        float3 v = (face.Normal * face.Distance).CartesianToBarycentric(
            vertices[face.VertexId.X].Value, vertices[face.VertexId.Y].Value, vertices[face.VertexId.Z].Value
        );

        /*float3 a = vertices[face.VertexIdA].Value;
        float3 b = vertices[face.VertexIdB].Value;
        float3 c = vertices[face.VertexIdC].Value;
        float3 pt = float3.Zero;

        float3 ab = b - a;
        float3 ac = c - a;
        float3 ap = pt - a;
        float3 bp = pt - b;
        float3 cp = pt - c;
        float3 bc = c - b;

        float3 n = ab.Cross(ac);
        float vc = n.Dot(ab.Cross(ap));
        float vb = -n.Dot(ac.Cross(cp));
        float va = n.Dot(bc.Cross(bp));*/

        /*float denom = 1 / (face.BarycentricData.X + face.BarycentricData.Y + face.BarycentricData.Z);
        float v = face.BarycentricData.Y * denom;
        float w = face.BarycentricData.Z * denom;
        float u = 1 - v - w;*/

        float3 pointA = vertices[face.VertexId.X].OriginA * v.X +
            vertices[face.VertexId.Y].OriginA * v.Y +
            vertices[face.VertexId.Z].OriginA * v.Z;
        float3 pointB = vertices[face.VertexId.X].OriginB * v.X +
            vertices[face.VertexId.Y].OriginB * v.Y +
            vertices[face.VertexId.Z].OriginB * v.Z;

        return new EpaResult(pointA, face.Normal, pointA.Distance(pointB));
    }

}
