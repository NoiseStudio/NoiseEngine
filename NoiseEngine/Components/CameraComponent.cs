using NoiseEngine.Jobs;

namespace NoiseEngine.Components {
    public readonly struct CameraComponent : IEntityComponent {

        public RenderCamera RenderCamera { get; }

        internal CameraComponent(RenderCamera camera) {
            RenderCamera = camera;
        }

        /// <summary>
        /// Casts <paramref name="component"/> to <see cref="RenderCamera"/>.
        /// </summary>
        /// <param name="component"><see cref="CameraComponent"/> to cast.</param>
        public static implicit operator RenderCamera(CameraComponent component) {
            return component.RenderCamera;
        }

    }
}
