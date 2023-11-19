using NoiseEngine.Mathematics;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics.Collision;

internal record struct ContactData(
    float3 OtherVelocity, float3 OtherAngularVelocity, Quaternion<float> OtherRotation, pos3 OtherPosition, float OtherInverseMass,
    float MinRestitutionPlusOneNegative, Matrix3x3<float> OtherInverseInertiaTensorMatrix
) {

    public ContactManifold Manifold;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddContactPoint(
        in ColliderTransform current, in ColliderTransform other, float restitutionPlusOneNegative, pos3 position,
        float3 normal, float depth
    ) {
        Manifold.AddContactPoint(new ContactPoint(position, default, normal, depth, default, default));
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
        OtherRotation = other.Rotation;
        OtherInverseMass = other.InverseMass;
        MinRestitutionPlusOneNegative = minRestitutionPlusOneNegative;
        OtherInverseInertiaTensorMatrix = other.InverseInertiaTensorMatrix;
    }

}
