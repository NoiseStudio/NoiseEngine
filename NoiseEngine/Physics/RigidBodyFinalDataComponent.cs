using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;

namespace NoiseEngine.Physics;

internal record struct RigidBodyFinalDataComponent(
    Vector3<float> LastPosition,
    Vector3<float> TargetPosition
) : IComponent;
