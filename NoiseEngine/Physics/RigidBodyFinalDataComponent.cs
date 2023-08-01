using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;

namespace NoiseEngine.Physics;

internal record struct RigidBodyFinalDataComponent(
    float MaxDistance,
    Vector3<float> LastPosition,
    Vector3<float> TargetPosition
) : IComponent;
