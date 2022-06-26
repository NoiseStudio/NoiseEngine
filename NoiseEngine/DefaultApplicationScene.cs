using NoiseEngine.Jobs;

namespace NoiseEngine {
    internal class DefaultApplicationScene : ApplicationScene {

        public override EntityWorld EntityWorld { get; } = new EntityWorld();

        protected override bool ReuseWindow(CameraData window, out Entity newCameraEntity) {
            if (!window.IsMainWindow) {
                newCameraEntity = default;
                return false;
            }

            newCameraEntity = EntityWorld.NewEntity();
            return true;
        }

        protected override void OnTerminate() {
            EntityWorld.Dispose();
        }

    }
}
