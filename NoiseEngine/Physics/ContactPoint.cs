using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics;

public readonly record struct ContactPoint {

    public pos3 Position { get; }
    public float3 Normal { get; }
    public float Depth { get; }

    internal float3 OtherVelocity { get; }
    internal pos3 OtherPosition { get; }
    internal float3 OtherAngularVelocity { get; }
    internal float OtherInverseMass { get; }
    internal float3 ResolveImpulseB { get; }
    internal float MinRestitutionPlusOneNegative { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ContactPoint(
        pos3 position, float3 normal, float depth, float3 otherVelocity, pos3 otherPosition, float3 otherAngularVelocity, float otherInverseMass, float inverseMass,
        float3 resolveImpulseB, float minRestitutionPlusOneNegative
    ) {
        Position = position;
        Normal = normal;
        Depth = depth;
        OtherVelocity = otherVelocity;
        OtherPosition = otherPosition;
        OtherAngularVelocity = otherAngularVelocity;
        OtherInverseMass = otherInverseMass;
        ResolveImpulseB = resolveImpulseB;
        MinRestitutionPlusOneNegative = minRestitutionPlusOneNegative;
    }

}
