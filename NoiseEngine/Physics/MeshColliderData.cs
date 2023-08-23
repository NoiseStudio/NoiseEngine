using System;
using NoiseEngine.Physics.Collision.Mesh;

namespace NoiseEngine.Physics;

public class MeshColliderData {

    public const int MaxVerticesPerHull = 256;

    private readonly ConvexHullId[] hullIds;
    private readonly float3[] vertices;

    private MeshColliderData(float3[] vertices, ConvexHullId[] hullIds) {
        this.vertices = vertices;
        this.hullIds = hullIds;
    }

    /// <summary>
    /// Unsafe creates <see cref="MeshColliderData"/> from <paramref name="convexHulls"/>.
    /// </summary>
    /// <param name="convexHulls"><see cref="ConvexHull"/>s of new <see cref="MeshColliderData"/>.</param>
    /// <returns>Unsafe created <see cref="MeshColliderData"/> from <paramref name="convexHulls"/>.</returns>
    public static MeshColliderData UnsafeCreateFromConvexHulls(ReadOnlySpan<ConvexHull> convexHulls) {
        int count = 0;
        foreach (ConvexHull hull in convexHulls)
            count += hull.Vertices.Length;

        float3[] vertices = new float3[count];
        ConvexHullId[] hullIds = new ConvexHullId[convexHulls.Length];

        int index = 0;
        for (int i = 0; i < convexHulls.Length; i++) {
            ConvexHull hull = convexHulls[i];
            hullIds[i] = new ConvexHullId(index, index + hull.Vertices.Length, hull.SphereCenter, hull.SphereRadius);
            hull.Vertices.CopyTo(vertices.AsMemory(index));
            index += hull.Vertices.Length;
        }

        return new MeshColliderData(vertices, hullIds);
    }

    internal MeshColliderDataValue GetValueData() {
        return new MeshColliderDataValue(hullIds, vertices);
    }

}
