using NoiseEngine.Mathematics;
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

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public PolytopeFace(Span<SupportPoint> vertices, int3 vertexId, int3 neighborId) {
        VertexId = vertexId;
        NeighborId = neighborId;

        float3 a = vertices[vertexId.X].Value;
        float3 ba = vertices[vertexId.Y].Value - a;
        float3 ca = vertices[vertexId.Z].Value - a;
        Normal = ba.Cross(ca).Normalize();
        Debug.Assert(Normal.MagnitudeSquared() > 0, "Normal is zero vector.");

        Distance = Normal.Dot(a);

        /*if (Distance < -0.00001f) {
            VertexIdB = vertexIdA;
            VertexIdA = vertexIdC;
            Normal = -Normal;
            Distance = Normal.Dot(vertices[vertexIdB].Value);
            Debug.Assert(Distance > 0);
        }*/
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

}
