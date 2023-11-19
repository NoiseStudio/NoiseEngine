using NoiseEngine.Mathematics;
using NoiseEngine.Physics.Collision.Mesh;
using System;

namespace NoiseEngine.Tests.Physics.Collision.Mesh;

internal readonly record struct GjkTestData(
    Isometry3<float> Pos12, ConvexHullId HullA, float3 ScaleA, ConvexHullId HullB, float3 ScaleB, float3[] VerticesA,
    float3[] VerticesB,
    bool Result, Simplex3D ResultSimplex,
    float3 EpaPosition, float3 EpaNormal, float EpaDepth
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
                new Isometry3<float>(new float3(-2, 0, 0)).InverseMultiplication(new Isometry3<float>(float3.Zero)),
                cubeHullId, float3.One, cubeHullId, float3.One, cubeVertices,
                cubeVertices, false, default,
                // EPA
                default, default, default
            ),
            2 => new GjkTestData(
                new Isometry3<float>(new float3(-.5f, -.5f, -.5f)).InverseMultiplication(
                    new Isometry3<float>(float3.Zero)
                ),
                cubeHullId, float3.One, cubeHullId, float3.One, cubeVertices,
                cubeVertices,
                true, new Simplex3D() {
                    A = new SupportPoint(new float3(.5f, .5f, .5f), new float3(.5f, .5f, .5f), float3.Zero),
                    B = new SupportPoint(new float3(-1.5f, -1.5f, -1.5f), new float3(-.5f, -.5f, -.5f), float3.One),
                },
                // EPA
                new float3(0.25f, 0.5f, 0.25f), new float3(0, 1, 0), 0.5f
            ),
            3 => new GjkTestData(
                new Isometry3<float>(new float3(0f, .5f, 0f)).InverseMultiplication(
                    new Isometry3<float>(float3.Zero)
                ),
                cubeHullId, float3.One, cubeHullId, float3.One, cubeVertices,
                cubeVertices,
                true, new Simplex3D() {
                    A = new SupportPoint(
                        new float3(0f, -.5f, 0f), new float3(-.5f, -.5f, -.5f), new float3(-.5f, 0f, -.5f)
                    ),
                    B = new SupportPoint(
                        new float3(0f, 1.5f, 0f), new float3(-.5f, .5f, -.5f), new float3(-.5f, -1f, -.5f)
                    ),
                },
                // EPA
                new float3(0f, -0.5f, 0f), new float3(0, 1, 0), 0.5f
            ),
            _ => throw new NotImplementedException()
        };
    }

}
