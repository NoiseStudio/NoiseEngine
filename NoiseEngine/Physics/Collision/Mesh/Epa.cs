using NoiseEngine.Mathematics;
using System;

namespace NoiseEngine.Physics.Collision.Mesh;

internal static class Epa {

    public const int MaxIterations = 64;
    private const float Epsilon = 0.0001f;

    public static EpaResult Process(
        in Simplex3D simplex, in float3 offsetA, in ConvexHullId hullA, in float3 scaleA, in ConvexHullId hullB,
        in float3 scaleB, ReadOnlySpan<float3> verticesA, ReadOnlySpan<float3> verticesB
    ) {
        Span<SupportPoint> polytopeVertices = stackalloc SupportPoint[MaxIterations];
        simplex.CopyTo(polytopeVertices);
        Span<PolytopeFace> polytopeFaces = stackalloc PolytopeFace[MaxIterations * 3];
        Polytope3D polytope = new Polytope3D(polytopeVertices, polytopeFaces);

        int i = 0;
        while (true) {
            PolytopeFace face = polytope.ClosestFaceToOrigin();
            SupportPoint supportPoint = Minkowski.FindSupportPoint(
                face.Normal, offsetA, hullA, scaleA, hullB, scaleB, verticesA, verticesB
            );

            float distance = supportPoint.Value.Dot(face.Normal);
            if (distance - face.Distance < Epsilon || i++ >= MaxIterations)
                return new EpaResult(ComputeContactPoint(polytope, face), face.Normal, face.Distance);

            polytope.Add(supportPoint);
        }
    }

    private static float3 ComputeContactPoint(in Polytope3D polytope, in PolytopeFace face) {
        Span<SupportPoint> vertices = polytope.Vertices;
        float3 v = (face.Normal * face.Distance).Barycentric(
            vertices[face.VertexIdA].Value, vertices[face.VertexIdB].Value, vertices[face.VertexIdC].Value
        );

        return vertices[face.VertexIdA].A * v.X +
            vertices[face.VertexIdB].A * v.Y +
            vertices[face.VertexIdC].A * v.Z;
    }

}
