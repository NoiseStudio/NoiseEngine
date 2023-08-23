using System;

namespace NoiseEngine.Physics.Collision.Mesh;

internal readonly ref struct MeshColliderDataValue {

    public readonly ReadOnlySpan<ConvexHullId> HullIds;
    public readonly ReadOnlySpan<float3> Vertices;

    public MeshColliderDataValue(ReadOnlySpan<ConvexHullId> hullIds, ReadOnlySpan<float3> vertices) {
        HullIds = hullIds;
        Vertices = vertices;
    }

}
