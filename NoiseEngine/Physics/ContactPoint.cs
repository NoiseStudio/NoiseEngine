using NoiseEngine.Mathematics;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics;

public readonly record struct ContactPoint {

    public pos3 Position { get; }
    public float3 Normal { get; }
    public float Depth { get; }

    internal float3 OtherVelocity { get; }
    internal float InverseMass { get; }
    internal float ResolveImpulseB { get; }
    internal float MinRestitutionPlusOneNegative { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ContactPoint(
        pos3 position, float3 normal, float depth, float3 otherVelocity, float inverseMass, float resolveImpulseB,
        float minRestitutionPlusOneNegative
    ) {
        Position = position;
        Normal = normal;
        Depth = depth;
        OtherVelocity = otherVelocity;
        InverseMass = inverseMass;
        ResolveImpulseB = resolveImpulseB;
        MinRestitutionPlusOneNegative = minRestitutionPlusOneNegative;
    }

}
