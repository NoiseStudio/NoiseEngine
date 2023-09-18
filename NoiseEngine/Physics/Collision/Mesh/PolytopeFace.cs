﻿using NoiseEngine.Mathematics;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics.Collision.Mesh;

internal struct PolytopeFace {

    public readonly int3 VertexId;
    public readonly float3 Normal;
    public readonly float Distance;
    public int3 NeighborId;
    public bool IsDeleted;
    public float3 BarycentricData;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public PolytopeFace(Span<SupportPoint> vertices, int3 vertexId, int3 neighborId) {
        VertexId = vertexId;
        NeighborId = neighborId;

        float3 a = vertices[vertexId.X].Value;
        float3 ba = vertices[vertexId.Y].Value - a;
        float3 ca = vertices[vertexId.Z].Value - a;
        Normal = ba.Cross(ca).Normalize();
        //Debug.Assert(Normal.MagnitudeSquared() > 0, "Normal is zero vector.");

        Distance = Normal.Dot(a);

        if (Distance < -0.00001f) {
            VertexId = VertexId with { Y = vertexId.X, X = vertexId.Y };
            Normal = -Normal;
            Distance = Normal.Dot(vertices[vertexId.Y].Value);
            Debug.Assert(Distance > 0);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public int3 NextCcw(Span<PolytopeFace> faces) {
        return new int3(
            faces[NeighborId.X].NextCcwInner(VertexId.X),
            faces[NeighborId.Y].NextCcwInner(VertexId.Y),
            faces[NeighborId.Z].NextCcwInner(VertexId.Z)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public int NextCcwInner(int vertexId) {
        if (VertexId.X == vertexId)
            return 1;
        if (VertexId.Y == vertexId)
            return 2;
        Debug.Assert(VertexId.Z == vertexId);
        return 0;
    }

    public float3? IsInside(Span<SupportPoint> vertices) {
        float3 a = vertices[VertexId.X].Value;
        float3 b = vertices[VertexId.Y].Value;
        float3 c = vertices[VertexId.Z].Value;

        float3 ab = b - a;
        float3 ac = c - a;
        float3 ap = -a;

        float abAp = ab.Dot(ap);
        float acAp = ac.Dot(ap);

        if (abAp < 0 && acAp < 0)
            return null;

        float3 bp = -b;
        float abBp = ab.Dot(bp);
        float acBp = ac.Dot(bp);

        if (abBp >= 0 && acBp < abBp)
            return null;

        float3 cp = -c;
        float abCp = ab.Dot(cp);
        float acCp = ac.Dot(cp);

        if (acCp >= 0 && abCp < acCp)
            return null;

        float3 n = ab.Cross(ac);

        float vc = n.Dot(ab.Cross(ap));
        if (vc < 0 && abAp >= 0 && abBp <= 0)
            return null;

        float vb = -n.Dot(ac.Cross(cp));
        if (vb < 0 && acAp >= 0 && acCp <= 0)
            return null;

        float3 bc = c - b;
        float va = n.Dot(bc.Cross(bp));
        if (va < 0 && acBp - abBp >= 0 && abCp - acCp >= 0)
            return null;

        return new float3(va, vb, vc);
    }

}
