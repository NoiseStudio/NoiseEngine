using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using System;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics.Collision.Mesh;

internal static class MeshToMesh {

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void Collide(
        EntityWorld world, SystemCommands commands, ContactPointsBuffer buffer, ContactDataBuffer dataBuffer, in MeshCollider current,
        float currentRestitutionPlusOneNegative, in ColliderTransform currentTransform, Entity currentEntity,
        MeshCollider other, float otherRestitutionPlusOneNegative, in ColliderTransform otherTransform,
        Entity otherEntity, Polytope3DBuffer polytopeBuffer
    ) {
        Isometry3<pos> posA = new Isometry3<pos>(currentTransform.Position, currentTransform.Rotation.ToPos());
        Isometry3<pos> posB = new Isometry3<pos>(otherTransform.Position, otherTransform.Rotation.ToPos());
        Isometry3<float> offsetA = posA.InverseMultiplication(posB).ToFloat();

        float currentScaleMax = currentTransform.Scale.MaxComponent();
        float otherScaleMax = otherTransform.Scale.MaxComponent();

        MeshColliderDataValue valueA = current.Data.GetValueData();
        MeshColliderDataValue valueB = other.Data.GetValueData();

        for (int i = 0; i < valueA.HullIds.Length; i++) {
            ref readonly ConvexHullId hullA = ref valueA.HullIds[i];
            float3 sphereCenterA = (hullA.SphereCenter * currentScaleMax) + offsetA.Translation;
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

                    pos3 pointA = epa.PositionA.ToPos();
                    //pos3 pointA = posA * epa.PositionA.ToPos();
                    float3 normalA = currentTransform.Rotation * epa.Normal;

                    float depth = epa.Depth;
                    float3 jB;

                    int convexHullId = i << 16 | j;
                    ref ContactData contactDataA = ref buffer.GetData(currentEntity, otherEntity, convexHullId);
                    float restitutionPlusOneNegative =
                        MathF.Max(currentRestitutionPlusOneNegative, otherRestitutionPlusOneNegative);

                    if (otherTransform.IsMovable) {
                        ref ContactData contactDataB = ref buffer.GetData(otherEntity, currentEntity, convexHullId);

                        depth *= 0.5f;

                        // A.
                        Matrix3x3<float> rotation = Matrix3x3<float>.Rotate(otherTransform.Rotation);
                        Matrix3x3<float> inverseInertia =
                            rotation * otherTransform.InverseInertiaTensorMatrix * rotation.Transpose();
                        float3 rb = (pointA - otherTransform.WorldCenterOfMass).ToFloat();
                        jB = (inverseInertia * rb.Cross(normalA)).Cross(rb);

                        // B.
                        pos3 pointB = posB * epa.PositionB.ToPos();
                        float3 normalB = -normalA;

                        Matrix3x3<float> rotation2 = Matrix3x3<float>.Rotate(currentTransform.Rotation);
                        Matrix3x3<float> inverseInertia2 =
                            rotation2 * currentTransform.InverseInertiaTensorMatrix * rotation2.Transpose();
                        float3 ra = (pointB - currentTransform.WorldCenterOfMass).ToFloat();
                        float3 jA = (inverseInertia2 * ra.Cross(normalB)).Cross(ra);

                        contactDataB.AddContactPoint(
                            otherTransform, currentTransform, restitutionPlusOneNegative, pointB, normalB, depth
                        );
                    } else {
                        jB = default;

                        if (otherTransform.IsRigidBody)
                            commands.GetEntity(otherEntity).Insert(new RigidBodySleepComponent(true));
                    }

                    contactDataA.AddContactPoint(
                        currentTransform, otherTransform, restitutionPlusOneNegative, pointA, normalA, depth
                    );

                    pos3 pointC = posB * epa.PositionB.ToPos();

                    var distance = (posA * pointA - pointC).ToFloat().Dot(-(otherTransform.Rotation * epa.Normal));

                    dataBuffer.AddContactPoint(currentEntity, otherEntity, convexHullId, new ContactPoint(
                        pointA, epa.PositionB, normalA, depth, jB, currentTransform.Position
                    ) {
                        StartDepth = depth
                    });

                    /*buffer.Add(currentEntity, new ContactPoint(
                        pointA,
                        normalA,
                        depth,
                        otherTransform.LinearVelocity,
                        otherTransform.Position,
                        otherTransform.AngularVelocity,
                        otherTransform.InverseMass,
                        currentTransform.InverseMass,
                        jB,
                        MathF.Max(currentRestitutionPlusOneNegative, otherRestitutionPlusOneNegative)
                    ));*/
                }
            }
        }
    }

}
