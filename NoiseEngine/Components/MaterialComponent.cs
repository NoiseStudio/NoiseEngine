using NoiseEngine.Jobs;
using NoiseEngine.Rendering;

namespace NoiseEngine.Components {
    public readonly record struct MaterialComponent(Material Material) : IEntityComponent {
    }
}
