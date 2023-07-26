using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics.Collision.Sphere;

internal static class SphereToSphere {

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Collide(
        ContactPointsBuffer buffer, SphereCollider current, ColliderTransform currentTransform, Entity currentEntity,
        SphereCollider other, ColliderTransform otherTransform
    ) {
        float c = (current.ScaledRadius(currentTransform.Scale) + other.ScaledRadius(otherTransform.Scale)) / 2;
        if (currentTransform.Position.DistanceSquared(otherTransform.Position) > c * c)
            return;

        buffer.Add(currentEntity, new ContactPoint(
            (otherTransform.Position - currentTransform.Position).Normalize()
        ));
    }

}
