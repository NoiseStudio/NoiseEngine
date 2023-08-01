using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;

namespace NoiseEngine.Physics;

public readonly record struct ContactPoint(
    Vector3<float> Normal,
    float Depth,
    Vector3<float> OtherVelocity,
    bool OtherIsRigidBody,
    Entity OtherEntity
);
