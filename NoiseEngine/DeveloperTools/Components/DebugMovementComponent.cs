using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;

namespace NoiseEngine.DeveloperTools.Components {
    public readonly record struct DebugMovementComponent(
        Float2 MouseRotation,
        float TimeUntilLastChangedPosition
    ) : IEntityComponent {
    }
}
