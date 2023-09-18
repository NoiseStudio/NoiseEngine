using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using System;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics.Collision.Sphere;

internal static class SphereToSphere {

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Collide(
        SystemCommands commands, ContactPointsBuffer buffer, in SphereCollider current,
        float currentRestitutionPlusOneNegative, in ColliderTransform currentTransform, Entity currentEntity,
        SphereCollider other, float otherRestitutionPlusOneNegative, in ColliderTransform otherTransform,
        Entity otherEntity
    ) {
        float currentRadius = current.ScaledRadius(currentTransform.Scale);
        float c = currentRadius + other.ScaledRadius(otherTransform.Scale);

        float3 normal = (otherTransform.Position - currentTransform.Position).ToFloat();
        float squaredDistance = normal.MagnitudeSquared();
        if (squaredDistance > c * c)
            return;

        float depth = c - MathF.Sqrt(squaredDistance);
        normal = normal.Normalize();
        pos3 contactPoint;
        float jB;

        if (otherTransform.IsMovable) {
            depth *= 0.5f;
            contactPoint = (normal * (currentRadius - depth)).ToPos() + currentTransform.Position;

            float3 rb = (contactPoint - otherTransform.WorldCenterOfMass).ToFloat();
            jB = otherTransform.InverseMass + normal.Dot(
                (otherTransform.InverseInertiaTensorMatrix * normal.Cross(rb)).Cross(rb)
            );
        } else {
            contactPoint = (normal * (currentRadius - (depth * 0.5f))).ToPos() + currentTransform.Position;
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
