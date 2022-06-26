using NoiseEngine.Jobs;
using NoiseEngine.Rendering;

namespace NoiseEngine.Components {
    public readonly struct CameraComponent : IEntityComponent {

        private readonly CameraData data;

        public Camera Camera => data.Camera;

        public CameraComponent(CameraData data) {
            this.data = data;
        }

    }
}
