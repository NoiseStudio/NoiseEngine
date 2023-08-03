using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using System;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics.Collision.Sphere;

internal static class SphereToSphere {

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Collide(
        ContactPointsBuffer buffer, SphereCollider current, ColliderTransform currentTransform, Entity currentEntity,
        SphereCollider other, ColliderTransform otherTransform, Entity otherEntity
    ) {
        float c = (current.ScaledRadius(currentTransform.Scale) + other.ScaledRadius(otherTransform.Scale)) / 2;
        float squaredDistance = currentTransform.Position.DistanceSquared(otherTransform.Position);
        if (squaredDistance > c * c)
            return;

        Vector3<float> normal = (otherTransform.Position - currentTransform.Position).Normalize();

        float depth = c - MathF.Sqrt(squaredDistance);
        if (otherTransform.IsRigidBody)
            depth /= 2;

        buffer.Add(currentEntity, new ContactPoint(
            normal,
            depth,
            otherTransform.Velocity,
            otherTransform.IsRigidBody,
            otherEntity
        ));
    }

}
