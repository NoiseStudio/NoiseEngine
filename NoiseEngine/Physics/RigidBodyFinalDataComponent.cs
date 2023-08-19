using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;

namespace NoiseEngine.Physics;

internal record struct RigidBodyFinalDataComponent(
    float SmoothingMultipler,
    pos3 LastPosition,
    pos3 TargetPosition,
    Quaternion<float> LastRotation,
    Quaternion<float> TargetRotation
) : IComponent;
