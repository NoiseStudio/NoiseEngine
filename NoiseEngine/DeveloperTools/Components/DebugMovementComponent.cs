using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;

namespace NoiseEngine.DeveloperTools.Components;

public readonly record struct DebugMovementComponent(
    Vector2<float> MouseRotation, float TimeUntilLastChangedPosition
) : IComponent;
