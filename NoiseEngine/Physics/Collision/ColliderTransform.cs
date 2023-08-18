using NoiseEngine.Mathematics;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics.Collision;

internal readonly record struct ColliderTransform(
    pos3 Position,
    pos3 WorldCenterOfMass,
    float3 Scale,
    float3 LinearVelocity,
    Matrix3x3<float> InverseInertiaTensorMatrix,
    float InverseMass,
    bool IsRigidBody
) {

    public bool IsMovable {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => InverseMass != 0;
    }

}
