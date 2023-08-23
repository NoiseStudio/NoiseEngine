using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using System;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics.Collision.Mesh;

internal static class MeshToMesh {

    private const int GjkMaxIterations = 256;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void Collide(
        EntityWorld world, SystemCommands commands, ContactPointsBuffer buffer, in MeshCollider current,
        float currentRestitutionPlusOneNegative, in ColliderTransform currentTransform, Entity currentEntity,
        MeshCollider other, float otherRestitutionPlusOneNegative, in ColliderTransform otherTransform,
        Entity otherEntity
    ) {
        float3 offsetA = (currentTransform.Position - otherTransform.Position).ToFloat();
        float currentScaleMax = currentTransform.Scale.MaxComponent();
        float otherScaleMax = otherTransform.Scale.MaxComponent();

        MeshColliderDataValue valueA = current.Data.GetValueData();
        MeshColliderDataValue valueB = other.Data.GetValueData();

        for (int i = 0; i < valueA.HullIds.Length; i++) {
            ref readonly ConvexHullId hullA = ref valueA.HullIds[i];
            float3 sphereCenterA = hullA.SphereCenter * currentScaleMax + offsetA;
            float sphereRadiusA = hullA.SphereRadius * currentScaleMax;

            for (int j = 0; j < valueB.HullIds.Length; j++) {
                ref readonly ConvexHullId hullB = ref valueB.HullIds[j];
                float3 sphereCenterB = hullB.SphereCenter * otherScaleMax;
                float sphereRadiusB = hullB.SphereRadius * otherScaleMax;

                float sphereRadiusSum = sphereRadiusA + sphereRadiusB;
                if (sphereCenterA.DistanceSquared(sphereCenterB) > sphereRadiusSum * sphereRadiusSum)
                    continue;

                if (CollideHull(
                    in offsetA, in hullA, currentTransform.Scale, in hullB, otherTransform.Scale, valueA.Vertices,
                    valueB.Vertices, out Simplex3D simplex
                )) {
                    EpaResult epa = Epa.Process(
                        simplex, in offsetA, in hullA, currentTransform.Scale, in hullB, otherTransform.Scale,
                        valueA.Vertices, valueB.Vertices
                    );
                    Log.Info($"C {currentTransform.Position - new pos3(0, -0.5f, 0)}");
                    pos3 pos = epa.Position.ToPos() + otherTransform.Position;
                    Log.Info($"E {pos}");

                    ((ApplicationScene)world).Primitive.CreateSphere(pos, Quaternion.EulerDegrees(epa.Normal * 180), (float3.One + epa.Normal) * epa.Depth);

                    /*float depth = 0.3f;
                    pos3 contactPoint = pos;
                    float3 normal = new float3(0, -1, 0);
                    //epa.Normal;
                    float jB;

                    if (otherTransform.IsMovable) {
                        depth *= 0.5f;

                        float3 rb = (contactPoint - otherTransform.WorldCenterOfMass).ToFloat();
                        jB = otherTransform.InverseMass + normal.Dot(
                            (otherTransform.InverseInertiaTensorMatrix * normal.Cross(rb)).Cross(rb)
                        );
                    } else {
                        jB = 0;

                        if (otherTransform.IsRigidBody)
                            commands.GetEntity(otherEntity).Insert(new RigidBodySleepComponent(true));
                    }

                    buffer.Add(currentEntity, new ContactPoint(
                        contactPoint,
                        normal,
                        depth,
                        otherTransform.LinearVelocity,
                        currentTransform.InverseMass,
                        jB,
                        MathF.Max(currentRestitutionPlusOneNegative, otherRestitutionPlusOneNegative)
                    ));*/
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static bool CollideHull(
        in float3 offsetA, in ConvexHullId hullA, in float3 scaleA, in ConvexHullId hullB, in float3 scaleB,
        ReadOnlySpan<float3> verticesA, ReadOnlySpan<float3> verticesB, out Simplex3D simplex
    ) {
        simplex = new Simplex3D();

        // A
        float3 direction = FindFirstDirection(offsetA, hullA, hullB);
        if (Minkowski.TryFindSupportPoint(
            direction, offsetA, hullA, scaleA, hullB, scaleB, verticesA, verticesB, out SupportPoint supportPoint
        )) {
            return false;
        }
        simplex.A = supportPoint;

        // B
        direction = -direction;
        if (Minkowski.TryFindSupportPoint(
            direction, offsetA, hullA, scaleA, hullB, scaleB, verticesA, verticesB, out supportPoint
        )) {
            return false;
        }
        simplex.B = supportPoint;

        // C
        FindThirdDirection(ref direction, in simplex);
        if (Minkowski.TryFindSupportPoint(
            direction, offsetA, hullA, scaleA, hullB, scaleB, verticesA, verticesB, out supportPoint
        )) {
            return false;
        }
        simplex.C = supportPoint;

        // D
        FindFourthDirection(ref direction, in simplex);
        if (Minkowski.TryFindSupportPoint(
            direction, offsetA, hullA, scaleA, hullB, scaleB, verticesA, verticesB, out supportPoint
        )) {
            return false;
        }
        simplex.D = supportPoint;

        // Iteration
        int max = Math.Min(
            Math.Max(hullA.EndIndex - hullA.StartIndex, hullB.EndIndex - hullB.StartIndex) - 4, GjkMaxIterations
        );
        for (int i = 0; i < max; i++) {
            float3 da = simplex.D.Value - simplex.A.Value;
            float3 db = simplex.D.Value - simplex.B.Value;
            float3 d0 = -simplex.D.Value;

            direction = da.Cross(db);
            if (direction.Dot(d0) > 0) {
                if (Minkowski.TryFindSupportPoint(
                    direction, offsetA, hullA, scaleA, hullB, scaleB, verticesA, verticesB, out supportPoint
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
                    direction, offsetA, hullA, scaleA, hullB, scaleB, verticesA, verticesB, out supportPoint
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
                    direction, offsetA, hullA, scaleA, hullB, scaleB, verticesA, verticesB, out supportPoint
                )) {
                    return false;
                }

                simplex.B = simplex.C;
                simplex.C = simplex.D;
                simplex.D = supportPoint;
                continue;
            }

            return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static float3 FindFirstDirection(in float3 offsetA, in ConvexHullId hullA, in ConvexHullId hullB) {
        return hullA.SphereCenter + offsetA - hullB.SphereCenter;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static void FindThirdDirection(ref float3 direction, in Simplex3D simplex) {
        float3 ab = simplex.B.Value - simplex.A.Value;
        direction = ab.Cross(-simplex.A.Value).Cross(ab);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static void FindFourthDirection(ref float3 direction, in Simplex3D simplex) {
        float3 ac = simplex.C.Value - simplex.A.Value;
        float3 ab = simplex.B.Value - simplex.A.Value;
        direction = ac.Cross(ab);

        if (direction.Dot(-simplex.A.Value) < 0)
            direction = -direction;
    }

}
