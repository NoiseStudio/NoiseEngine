using NoiseEngine.Mathematics;
using NoiseEngine.Rendering;
using System;
using System.Collections.Generic;

namespace NoiseEngine.Primitives.Sphere;

internal class IcosphereMesh {

    private readonly Dictionary<int, ushort> midCache = new Dictionary<int, ushort>();
    private ushort v = 12;

    public Mesh GenerateMesh(PrimitiveCreatorShared shared, uint resolution) {
        int pow = (int)Math.Pow(4, resolution);
        Span<float> vertices = new float[(10 * pow + 2) * 3];
        FillStartingVertices(vertices);
        Span<ushort> finalIndices = new ushort[60 * pow];
        FillStartingIndices(finalIndices);
        Span<ushort> indices = new ushort[finalIndices.Length];

        int previousIndexCount = 60;
        for (int i = 0; i < resolution; i++) {
            Span<ushort> temp = finalIndices;
            finalIndices = indices;
            indices = temp;

            for (int j = 0; j < previousIndexCount; j += 3) {
                ushort v1 = indices[j];
                ushort v2 = indices[j + 1];
                ushort v3 = indices[j + 2];

                ushort a = AddMidPoint(vertices, v1, v2);
                ushort b = AddMidPoint(vertices, v2, v3);
                ushort c = AddMidPoint(vertices, v3, v1);

                int t = j * 4;

                finalIndices[t++] = v1;
                finalIndices[t++] = a;
                finalIndices[t++] = c;

                finalIndices[t++] = v2;
                finalIndices[t++] = b;
                finalIndices[t++] = a;

                finalIndices[t++] = v3;
                finalIndices[t++] = c;
                finalIndices[t++] = b;

                finalIndices[t++] = a;
                finalIndices[t++] = b;
                finalIndices[t++] = c;
            }

            previousIndexCount *= 4;
        }

        Span<VertexPosition3Color3> finalVertices = new VertexPosition3Color3[vertices.Length / 3];
        for (int i = 0; i < finalVertices.Length; i++) {
            int j = i * 3;

            Vector3<float> position = new Vector3<float>(vertices[j], vertices[j + 1], vertices[j + 2]);
            position *= 0.5f / position.Magnitude();

            finalVertices[i] = new VertexPosition3Color3(
                position,
                new Vector3<float>(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle())
            );
        }

        return new Mesh<VertexPosition3Color3, ushort>(shared.GraphicsDevice, finalVertices, finalIndices);
    }

    private ushort AddMidPoint(Span<float> vertices, ushort a, ushort b) {
        int key = (a + b) * (a + b + 1) / 2 + Math.Min(a, b);
        if (midCache.TryGetValue(key, out ushort i))
            return i;

        midCache.Add(key, v);
        for (int k = 0; k < 3; k++)
            vertices[v * 3 + k] = (vertices[a * 3 + k] + vertices[b * 3 + k]) / 2;

        return v++;
    }

    private void FillStartingVertices(Span<float> vertices) {
        const float A = 0.525731112119133606f;
        const float B = 0.850650808352039932f;

        stackalloc float[36] {
            -A, B, 0, A, B, 0, -A, -B, 0, A, -B, 0,
            0, -A, B, 0, A, B, 0, -A, -B, 0, A, -B,
            B, 0, -A, B, 0, A, -B, 0, -A, -B, 0, A
        }.CopyTo(vertices);
    }

    private void FillStartingIndices(Span<ushort> indices) {
        stackalloc ushort[60] {
            0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11,
            11, 10, 2, 5, 11, 4, 1, 5, 9, 7, 1, 8, 10, 7, 6,
            3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9,
            9, 8, 1, 4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7
        }.CopyTo(indices);
    }

}
