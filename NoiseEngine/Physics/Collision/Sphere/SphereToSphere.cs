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
        float c = current.ScaledRadius(currentTransform.Scale) + other.ScaledRadius(otherTransform.Scale);
        float squaredDistance = currentTransform.Position.DistanceSquared(otherTransform.Position);
        if (squaredDistance > c * c)
            return;

        float depth = c - MathF.Sqrt(squaredDistance);
        if (otherTransform.InverseMass != 0f)
            depth *= 0.5f;

        buffer.Add(currentEntity, new ContactPoint(
            (otherTransform.Position - currentTransform.Position).Normalize(),
            depth,
            otherTransform.LinearVelocity,
            currentTransform.InverseMass,
            currentTransform.InverseMass + otherTransform.InverseMass,
            MathF.Max(currentTransform.RestitutionPlusOneNegative, otherTransform.RestitutionPlusOneNegative)
        ));
    }

}
