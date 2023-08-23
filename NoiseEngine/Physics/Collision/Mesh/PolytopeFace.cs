using NoiseEngine.Mathematics;
using System;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics.Collision.Mesh;

internal readonly struct PolytopeFace {

    public readonly int VertexIdA;
    public readonly int VertexIdB;
    public readonly int VertexIdC;
    public readonly float3 Normal;
    public readonly float Distance;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public PolytopeFace(Span<SupportPoint> vertices, int vertexIdA, int vertexIdB, int vertexIdC) {
        VertexIdA = vertexIdA;
        VertexIdB = vertexIdB;
        VertexIdC = vertexIdC;

        float3 a = vertices[vertexIdA].Value;
        float3 ba = vertices[vertexIdB].Value - a;
        float3 ca = vertices[vertexIdC].Value - a;
        Normal = ba.Cross(ca).Normalize();

        Distance = Normal.Dot(a);
    }

}
