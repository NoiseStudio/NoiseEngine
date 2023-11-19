using NoiseEngine.Mathematics;
using NoiseEngine.Physics.Collision;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics;

internal record struct ContactPoint(pos3 Position, float3 PositionB, float3 Normal, float Depth, float3 ResolveImpulseB, pos3 Center) {

    public float StartDepth;
    public float MassNormal;
    public float Bias;
    public float PreviousNormalImpulse;

    public bool IsValid => Depth < 0.01f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ComputeResolveImpulse(in ColliderTransform current, Quaternion<float> a, pos3 b) {
        Matrix3x3<float> rotation = Matrix3x3<float>.Rotate(current.Rotation);
        Matrix3x3<float> inverseInertia =
            rotation * current.InverseInertiaTensorMatrix * rotation.Transpose();

        pos3 pos = (a.ToPos() * Position) + b;
        float3 rb = (pos - current.WorldCenterOfMass).ToFloat();
        float3 rbCrossN = rb.Cross(Normal);

        ResolveImpulseB = (inverseInertia * rb.Cross(Normal)).Cross(rb);
    }

}
