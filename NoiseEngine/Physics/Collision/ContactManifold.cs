namespace NoiseEngine.Physics.Collision;

internal readonly record struct ContactManifold(
    float3 OtherVelocity, float3 OtherAngularVelocity, pos3 OtherPosition, float OtherInverseMass,
    float MinRestitutionPlusOneNegative
);
