using System;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics.Collision.Mesh;

internal static class Minkowski {

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public static bool TryFindSupportPoint(
        float3 direction, in float3 offsetA, in ConvexHullId hullA, in float3 scaleA, in ConvexHullId hullB,
        in float3 scaleB, ReadOnlySpan<float3> verticesA, ReadOnlySpan<float3> verticesB,
        out SupportPoint supportPoint
    ) {
        supportPoint = FindSupportPoint(direction, offsetA, hullA, scaleA, hullB, scaleB, verticesA, verticesB);
        return direction.Dot(supportPoint.Value) < 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public static SupportPoint FindSupportPoint(
        float3 direction, in float3 offsetA, in ConvexHullId hullA, in float3 scaleA, in ConvexHullId hullB,
        in float3 scaleB, ReadOnlySpan<float3> verticesA, ReadOnlySpan<float3> verticesB
    ) {
        float3 a = FindFarthestPointInDirection(direction, hullA, verticesA).Scale(scaleA) + offsetA;
        return new SupportPoint(a - FindFarthestPointInDirection(-direction, hullB, verticesB).Scale(scaleB), a);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static float3 FindFarthestPointInDirection(
        in float3 direction, in ConvexHullId hull, ReadOnlySpan<float3> vertices
    ) {
        float maxDistance = float.MinValue;
        int index = 0;

        for (int i = hull.StartIndex; i < hull.EndIndex; i++) {
            float distance = vertices[i].Dot(direction);
            if (distance > maxDistance) {
                maxDistance = distance;
                index = i;
            }
        }

        return vertices[index];
    }

}
