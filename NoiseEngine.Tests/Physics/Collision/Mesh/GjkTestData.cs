using NoiseEngine.Physics.Collision.Mesh;
using System;

namespace NoiseEngine.Tests.Physics.Collision.Mesh;

internal readonly record struct GjkTestData(
    float3 OffsetA, ConvexHullId HullA, float3 ScaleA, ConvexHullId HullB, float3 ScaleB, float3[] VerticesA,
    float3[] VerticesB,
    bool Result, Simplex3D ResultSimplex
) {

    private static readonly float3[] cubeVertices = new float3[] {
        new float3(-.5f, -.5f, -.5f),
        new float3(.5f, -.5f, -.5f),
        new float3(-.5f, -.5f, .5f),
        new float3(.5f, -.5f, .5f),
        new float3(-.5f, .5f, -.5f),
        new float3(.5f, .5f, -.5f),
        new float3(-.5f, .5f, .5f),
        new float3(.5f, .5f, .5f)
    };

    private static readonly ConvexHullId cubeHullId = new ConvexHullId(0, 8, float3.Zero, float.Sqrt(3) * 0.5f);

    public static GjkTestData GetData(int index) {
        return index switch {
            1 => new GjkTestData(
                new float3(2f, 0f, 0f), cubeHullId, float3.One, cubeHullId, float3.One, cubeVertices,
                cubeVertices, false, default
            ),
            2 => new GjkTestData(
                new float3(-.5f, -.5f, -.5f), cubeHullId, float3.One, cubeHullId, float3.One, cubeVertices,
                cubeVertices,
                true, new Simplex3D() {
                    A = new SupportPoint(new float3(.5f, .5f, .5f), new float3(.5f, .5f, .5f)),
                    B = new SupportPoint(new float3(-1.5f, -1.5f, -1.5f), new float3(-.5f, -.5f, -.5f)),
                }
            ),
            _ => throw new NotImplementedException()
        };
    }

}
