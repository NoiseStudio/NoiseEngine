using NoiseEngine.Jobs;
using NoiseEngine.Rendering;
using System;

namespace NoiseEngine.Components;

public readonly record struct MeshRendererComponent : IComponent {

    public Mesh Mesh { get; }
    public Material Material { get; }

    /// <summary>
    /// Do not use default constructor for this type, always throws <see cref="InvalidOperationException"/>.
    /// Use another constructor instead.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Always throws <see cref="InvalidOperationException"/>.
    /// </exception>
    [Obsolete($"Do not use default constructor for this type. Use another constructor instead.", true)]
    public MeshRendererComponent() {
        throw new InvalidOperationException(
            "Do not use default constructor for this type. Use another constructor instead."
        );
    }

    public MeshRendererComponent(Mesh mesh, Material material) {
        Mesh = mesh;
        Material = material;
    }

}
