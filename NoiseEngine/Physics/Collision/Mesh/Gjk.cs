using NoiseEngine.Mathematics;
using System;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics.Collision.Mesh;

internal static class Gjk {

    private const int GjkMaxIterations = 256;

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public static bool Intersect(
        in Isometry3<float> pos12, in ConvexHullId hullA, in float3 scaleA, in ConvexHullId hullB, in float3 scaleB,
        ReadOnlySpan<float3> verticesA, ReadOnlySpan<float3> verticesB, out Simplex3D simplex
    ) {
        simplex = new Simplex3D();

        // A
        float3 direction = FindFirstDirection(pos12, hullA, hullB);
        if (Minkowski.TryFindSupportPoint(
            direction, pos12, hullA, scaleA, hullB, scaleB, verticesA, verticesB, out SupportPoint supportPoint
        )) {
            return false;
        }
        simplex.A = supportPoint;

        // B
        direction = -direction;
        if (Minkowski.TryFindSupportPoint(
            direction, pos12, hullA, scaleA, hullB, scaleB, verticesA, verticesB, out supportPoint
        )) {
            return false;
        }
        simplex.B = supportPoint;

        // C
        FindThirdDirection(ref direction, in simplex);
        if (direction == float3.Zero) {
            simplex.Dimensions = 1;
            return true;
        }

        if (Minkowski.TryFindSupportPoint(
            direction, pos12, hullA, scaleA, hullB, scaleB, verticesA, verticesB, out supportPoint
        )) {
            return false;
        }
        simplex.C = supportPoint;

        // D
        FindFourthDirection(ref direction, in simplex);
        if (direction == float3.Zero) {
            simplex.Dimensions = 2;
            return true;
        }

        if (Minkowski.TryFindSupportPoint(
            direction, pos12, hullA, scaleA, hullB, scaleB, verticesA, verticesB, out supportPoint
        )) {
            return false;
        }
        simplex.D = supportPoint;

        // Iteration
        for (int i = 0; i < GjkMaxIterations; i++) {
            float3 da = simplex.D.Value - simplex.A.Value;
            float3 db = simplex.D.Value - simplex.B.Value;
            float3 d0 = -simplex.D.Value;

            direction = da.Cross(db);
            if (direction.Dot(d0) > 0) {
                if (Minkowski.TryFindSupportPoint(
                    direction, pos12, hullA, scaleA, hullB, scaleB, verticesA, verticesB, out supportPoint
                )) {
                    return false;
                }

                simplex.C = simplex.D;
                simplex.D = supportPoint;
                continue;
            }

            float3 dc = simplex.D.Value - simplex.C.Value;
            direction = db.Cross(dc);
            if (direction.Dot(d0) > 0) {
                if (Minkowski.TryFindSupportPoint(
                    direction, pos12, hullA, scaleA, hullB, scaleB, verticesA, verticesB, out supportPoint
                )) {
                    return false;
                }

                simplex.A = simplex.B;
                simplex.B = simplex.C;
                simplex.C = simplex.D;
                simplex.D = supportPoint;
                continue;
            }

            direction = dc.Cross(da);
            if (direction.Dot(d0) > 0) {
                if (Minkowski.TryFindSupportPoint(
                    direction, pos12, hullA, scaleA, hullB, scaleB, verticesA, verticesB, out supportPoint
                )) {
                    return false;
                }

                simplex.B = simplex.C;
                simplex.C = simplex.D;
                simplex.D = supportPoint;
                continue;
            }

            simplex.Dimensions = 3;
            return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static float3 FindFirstDirection(
        in Isometry3<float> pos12, in ConvexHullId hullA, in ConvexHullId hullB
    ) {
        return (hullA.SphereCenter + pos12.Translation - hullB.SphereCenter).Normalize();
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static void FindThirdDirection(ref float3 direction, in Simplex3D simplex) {
        float3 ab = simplex.B.Value - simplex.A.Value;
        direction = ab.Cross(-simplex.A.Value).Cross(ab).Normalize();
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static void FindFourthDirection(ref float3 direction, in Simplex3D simplex) {
        float3 ac = simplex.C.Value - simplex.A.Value;
        float3 ab = simplex.B.Value - simplex.A.Value;
        direction = ac.Cross(ab).Normalize();

        if (direction.Dot(-simplex.A.Value) < 0)
            direction = -direction;
    }

}
