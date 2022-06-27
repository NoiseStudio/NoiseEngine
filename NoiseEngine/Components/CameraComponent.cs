using NoiseEngine.Jobs;

namespace NoiseEngine.Components {
    public readonly struct CameraComponent : IEntityComponent {

        public RenderCamera RenderCamera { get; }

        internal CameraComponent(RenderCamera camera) {
            RenderCamera = camera;
        }

        public static implicit operator RenderCamera(CameraComponent component) {
            return component.RenderCamera;
        }

    }
}
