using NoiseEngine.Physics.Collision.Mesh;
using System;

namespace NoiseEngine.Tests.Physics.Collision.Mesh;

public class EpaTest {

    [Fact]
    public void Process() {
        Simplex3D simplex = new Simplex3D() {
            A = new SupportPoint(new float3(18f, -12f, 0f), default),
            B = new SupportPoint(new float3(-2f, 8f, 0f), default),
            C = new SupportPoint(new float3(-2f, -12f, 0f), default),
            D = new SupportPoint(new float3(8f, -2f, -10f), default),
        };

        ReadOnlySpan<float3> vertices = stackalloc float3[] {
            new float3(-5f, -5f, -5f),
            new float3(5f, -5f, -5f),
            new float3(-5f, -5f, 5f),
            new float3(5f, -5f, 5f),
            new float3(-5f, 5f, -5f),
            new float3(5f, 5f, -5f),
            new float3(-5f, 5f, 5f),
            new float3(5f, 5f, 5f)
        };

        EpaResult result = Epa.Process(
            simplex, new float3(15, 0, 0) - new float3(7, 2, 0), new ConvexHullId(0, 8, default, default), float3.One,
            new ConvexHullId(0, 8, default, default), float3.One, vertices, vertices
        );

        Assert.Equal(new float3(-1, 0, 0), result.Normal);
        Assert.Equal(2, result.Depth);
    }

}
