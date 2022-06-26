using NoiseEngine.Jobs;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Presentation;
using NoiseEngine.Systems;
using System;

namespace NoiseEngine {
    public class CameraData : IDisposable {

        public bool IsDisposed { get; private set; }
        public Entity Entity { get; private set; }
        public MeshRendererSystem MeshRenderer { get; private set; }
        public Application Application { get; }
        public Window RenderTarget { get; }
        public Camera Camera { get; }

        public bool IsWindow => true;
        public bool IsMainWindow => Application.MainWindow == RenderTarget;
        public bool IsShouldClose => IsDisposed || (IsWindow && RenderTarget.GetShouldClose());

        internal CameraData(Application application, Window renderTarget, Camera camera) {
            Application = application;
            RenderTarget = renderTarget;
            Camera = camera;

            MeshRenderer = null!;
            CreateMeshRenderer();
        }

        public void Dispose() {
            IsDisposed = true;

            MeshRenderer.Dispose();
            RenderTarget.Destroy();
        }

        internal void UpdateScene() {
            MeshRendererSystem oldMeshRenderer = MeshRenderer;
            CreateMeshRenderer();
            oldMeshRenderer.Dispose();
        }

        internal void UpdateEntity(Entity entity) {
            Entity = entity;
        }

        private void CreateMeshRenderer() {
            MeshRenderer = new MeshRendererSystem(Application.GraphicsDevice, Camera);
            MeshRenderer.Initialize(Application.CurrentScene.EntityWorld, Application.EntitySchedule);
        }

    }
}
