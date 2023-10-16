using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics.Collision;

internal record struct ContactData(
    float3 OtherVelocity, float3 OtherAngularVelocity, pos3 OtherPosition, float OtherInverseMass,
    float MinRestitutionPlusOneNegative
) {

    public ContactManifold Manifold;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddContactPoint(
        in ColliderTransform current, in ColliderTransform other, float restitutionPlusOneNegative, pos3 position,
        float3 normal, float depth
    ) {
        Manifold.AddContactPoint(new ContactPoint(position, normal, depth, default));
        Manifold.ComputeResolveImpulse(in other, current.Rotation, current.Position);
        Update(in other, restitutionPlusOneNegative);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update(
        in ColliderTransform other, float minRestitutionPlusOneNegative
    ) {
        OtherVelocity = other.LinearVelocity;
        OtherAngularVelocity = other.AngularVelocity;
        OtherPosition = other.Position;
        OtherInverseMass = other.InverseMass;
        MinRestitutionPlusOneNegative = minRestitutionPlusOneNegative;
    }

}
