using NoiseEngine.Mathematics;
using NoiseEngine.Physics.Collision;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics;

internal record struct ContactPoint(pos3 Position, float3 Normal, float Depth, float3 ResolveImpulseB) {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ComputeResolveImpulse(in ColliderTransform current, Quaternion<float> a, pos3 b) {
        Matrix3x3<float> rotation = Matrix3x3<float>.Rotate(current.Rotation);
        Matrix3x3<float> inverseInertia =
            rotation * current.InverseInertiaTensorMatrix * rotation.Transpose();

        var pos = (a.ToPos() * Position) + b;
        float3 rb = (pos - current.WorldCenterOfMass).ToFloat();
        ResolveImpulseB = (inverseInertia * rb.Cross(Normal)).Cross(rb);
    }

}
