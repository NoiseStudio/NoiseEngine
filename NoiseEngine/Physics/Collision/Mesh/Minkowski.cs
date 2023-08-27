using NoiseEngine.Mathematics;
using System;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics.Collision.Mesh;

internal static class Minkowski {

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public static bool TryFindSupportPoint(
        float3 direction, in Isometry3<float> pos12, in ConvexHullId hullA, in float3 scaleA, in ConvexHullId hullB,
        in float3 scaleB, ReadOnlySpan<float3> verticesA, ReadOnlySpan<float3> verticesB,
        out SupportPoint supportPoint
    ) {
        supportPoint = FindSupportPoint(direction, pos12, hullA, scaleA, hullB, scaleB, verticesA, verticesB);
        return direction.Dot(supportPoint.Value) < 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public static SupportPoint FindSupportPoint(
        float3 direction, in Isometry3<float> pos12, in ConvexHullId hullA, in float3 scaleA, in ConvexHullId hullB,
        in float3 scaleB, ReadOnlySpan<float3> verticesA, ReadOnlySpan<float3> verticesB
    ) {
        float3 a = FindFarthestPointInDirectionLocal(direction, hullA, verticesA).Scale(scaleA);
        float3 b = FindFarthestPointInDirection(-direction, pos12, hullB, scaleB, verticesB);
        return new SupportPoint(a - b, a, b);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static float3 FindFarthestPointInDirection(
        in float3 direction, in Isometry3<float> pos12, in ConvexHullId hull, in float3 scale, ReadOnlySpan<float3> vertices
    ) {
        float3 localDirection = pos12.Rotation.Conjugate() * direction;
        return pos12 * FindFarthestPointInDirectionLocal(localDirection, hull, vertices).Scale(scale);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static float3 FindFarthestPointInDirectionLocal(
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
