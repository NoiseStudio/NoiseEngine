using NoiseEngine.Jobs;

namespace NoiseEngine.Components;

public readonly struct CameraComponent : IEntityComponent {

    public Camera Camera { get; }

    internal CameraComponent(Camera camera) {
        Camera = camera;
    }

    /// <summary>
    /// Casts <paramref name="component"/> to <see cref="Camera"/>.
    /// </summary>
    /// <param name="component"><see cref="CameraComponent"/> to cast.</param>
    public static implicit operator Camera(CameraComponent component) {
        return component.Camera;
    }

}
