using NoiseEngine.Mathematics;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics;

public readonly record struct ContactPoint {

    public Vector3<float> Position { get; }
    public Vector3<float> Normal { get; }
    public float Depth { get; }

    internal Vector3<float> OtherVelocity { get; }
    internal float InverseMass { get; }
    internal float ResolveImpulseB { get; }
    internal float MinRestitutionPlusOneNegative { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ContactPoint(
        Vector3<float> position, Vector3<float> normal, float depth, Vector3<float> otherVelocity, float inverseMass,
        float resolveImpulseB, float minRestitutionPlusOneNegative
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
