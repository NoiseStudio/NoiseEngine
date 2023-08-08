using NoiseEngine.Mathematics;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics.Collision;

internal readonly record struct ColliderTransform(
    Vector3<float> Position,
    Quaternion<float> Rotation,
    Vector3<float> Scale,
    Vector3<float> LinearVelocity,
    float InverseMass,
    // -(1f + Restitution)
    float RestitutionPlusOneNegative
) {

    public bool IsMovable {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => InverseMass != 0;
    }

}
