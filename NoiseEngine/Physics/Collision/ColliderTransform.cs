using NoiseEngine.Mathematics;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics.Collision;

internal readonly record struct ColliderTransform(
    Vector3<float> Position,
    Vector3<float> WorldCenterOfMass,
    Vector3<float> Scale,
    Vector3<float> LinearVelocity,
    Matrix3x3<float> InverseInertiaTensorMatrix,
    float InverseMass,
    // -(1f + Restitution)
    float RestitutionPlusOneNegative
) {

    public bool IsMovable {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => InverseMass != 0;
    }

}
