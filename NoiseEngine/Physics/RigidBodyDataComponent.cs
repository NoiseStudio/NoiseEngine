using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;

namespace NoiseEngine.Physics;

internal readonly record struct RigidBodyDataComponent(
    Vector3<float> LastPosition,
    Vector3<float> TargetPosition
) : IComponent;
