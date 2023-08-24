using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics.Collision.Mesh;

internal static class MeshToMesh {

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

                if (Gjk.Intersect(
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

}
