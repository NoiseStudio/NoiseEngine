using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using System;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics.Collision.Mesh;

internal static class MeshToMesh {

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void Collide(
        EntityWorld world, SystemCommands commands, ContactPointsBuffer buffer, in MeshCollider current,
        float currentRestitutionPlusOneNegative, in ColliderTransform currentTransform, Entity currentEntity,
        MeshCollider other, float otherRestitutionPlusOneNegative, in ColliderTransform otherTransform,
        Entity otherEntity, Polytope3DBuffer polytopeBuffer
    ) {
        Isometry3<pos> posA = new Isometry3<pos>(currentTransform.Position, currentTransform.Rotation.ToPos());
        Isometry3<float> offsetA = posA.ConjugateMultiplication(
            new Isometry3<pos>(otherTransform.Position, otherTransform.Rotation.ToPos())
        ).ToFloat();

        float currentScaleMax = currentTransform.Scale.MaxComponent();
        float otherScaleMax = otherTransform.Scale.MaxComponent();

        MeshColliderDataValue valueA = current.Data.GetValueData();
        MeshColliderDataValue valueB = other.Data.GetValueData();

        for (int i = 0; i < valueA.HullIds.Length; i++) {
            ref readonly ConvexHullId hullA = ref valueA.HullIds[i];
            float3 sphereCenterA = hullA.SphereCenter * currentScaleMax + offsetA.Translation;
            float sphereRadiusA = hullA.SphereRadius * currentScaleMax;

            for (int j = 0; j < valueB.HullIds.Length; j++) {
                ref readonly ConvexHullId hullB = ref valueB.HullIds[j];
                float3 sphereCenterB = hullB.SphereCenter * otherScaleMax;
                float sphereRadiusB = hullB.SphereRadius * otherScaleMax;

                float sphereRadiusSum = sphereRadiusA + sphereRadiusB;
                if (sphereCenterA.DistanceSquared(sphereCenterB) > sphereRadiusSum * sphereRadiusSum)
                    continue;

                if (Gjk.Intersect(
                    in offsetA, in hullA, currentTransform.Scale, in hullB, otherTransform.Scale, valueA.Vertices,
                    valueB.Vertices, out Simplex3D simplex
                )) {
                    EpaResult epa = Epa.Process(
                        simplex, in offsetA, in hullA, currentTransform.Scale, in hullB, otherTransform.Scale,
                        valueA.Vertices, valueB.Vertices, polytopeBuffer
                    );
                    //Log.Info($"C {currentTransform.Position - new pos3(0, -0.5f, 0)}");
                    pos3 pos = posA * epa.Position.ToPos();
                    //Log.Info($"E {pos}");

                    float3 normal = currentTransform.Rotation * epa.Normal;
                    ((ApplicationScene)world).Primitive.CreateSphere(pos + (normal * 0.3f).ToPos(), null, (float3.One + normal.Abs() * 3) / 10f);

                    float depth = epa.Depth;
                    pos3 contactPoint = pos;
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
                        otherTransform.Position,
                        otherTransform.AngularVelocity,
                        otherTransform.InverseMass,
                        currentTransform.InverseMass,
                        jB,
                        MathF.Max(currentRestitutionPlusOneNegative, otherRestitutionPlusOneNegative)
                    ));
                }
            }
        }
    }

}
