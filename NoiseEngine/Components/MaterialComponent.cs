using NoiseEngine.Jobs;
using NoiseEngine.Rendering;

namespace NoiseEngine.Components {
    public record struct MaterialComponent(Material Material) : IEntityComponent {
    }
}
