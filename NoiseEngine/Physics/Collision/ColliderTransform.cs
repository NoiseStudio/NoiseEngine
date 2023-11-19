using NoiseEngine.Mathematics;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics.Collision;

internal record struct ColliderTransform(
    pos3 Position,
    Quaternion<float> Rotation,
    pos3 WorldCenterOfMass,
    float3 Scale,
    float3 LinearVelocity,
    float3 AngularVelocity,
    Matrix3x3<float> InverseInertiaTensorMatrix,
    float InverseMass,
    bool IsRigidBody
) {

    public bool IsMovable {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => InverseMass != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public pos3 LocalToWorldPosition(float3 localPosition) {
        return (Rotation * localPosition).ToPos() + Position;
    }

}
