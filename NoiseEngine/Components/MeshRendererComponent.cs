using NoiseEngine.Jobs;
using NoiseEngine.Rendering;

namespace NoiseEngine.Components;

public readonly record struct MeshRendererComponent(Mesh Mesh) : IEntityComponent;
