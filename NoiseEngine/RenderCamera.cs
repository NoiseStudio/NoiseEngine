using NoiseEngine.Components;
using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Presentation;
using NoiseEngine.Systems;
using NoiseEngine.Threading;
using System;

namespace NoiseEngine {
    public class RenderCamera : IDisposable {

        private readonly MeshRendererSystem meshRenderer;

        private RenderCameraThread? thread;
        private AtomicBool isDisposed;

        public Entity Entity { get; }
        public ApplicationScene Scene { get; }
        public bool IsDisposed => isDisposed;

        public bool AutoRender {
            get => thread is not null;
            set {
                if (value) {
                    thread = new RenderCameraThread(Scene, Camera, meshRenderer);
                } else {
                    thread?.Dispose();
                    thread = null;
                }
            }
        }

        public ProjectionType ProjectionType {
            get => Camera.ProjectionType;
            set => Camera.ProjectionType = value;
        }

        public float NearClipPlane {
            get => Camera.NearClipPlane;
            set => Camera.NearClipPlane = value;
        }

        public float FarClipPlane {
            get => Camera.FarClipPlane;
            set => Camera.FarClipPlane = value;
        }

        public AngleFloat FieldOfView {
            get => Camera.FieldOfView;
            set => Camera.FieldOfView = value;
        }

        public float OrthographicSize {
            get => Camera.OrthographicSize;
            set => Camera.OrthographicSize = value;
        }

        public Window RenderTarget => Camera.RenderTarget;

        internal Camera Camera { get; }

        internal RenderCamera(ApplicationScene scene, Camera camera, bool autoRender) {
            Scene = scene;
            Camera = camera;

            Entity = scene.EntityWorld.NewEntity(
                new TransformComponent(camera.Position, camera.Rotation),
                new CameraComponent(this)
            );

            meshRenderer = new MeshRendererSystem(Scene.Application.GraphicsDevice, Camera);
            meshRenderer.Initialize(Scene.EntityWorld, Scene.Application.EntitySchedule);

            AutoRender = autoRender;
        }

        ~RenderCamera() {
            Dispose();
        }

        /// <summary>
        /// Disposes this <see cref="RenderCamera"/> and removes
        /// <see cref="CameraComponent"/> from <see cref="Entity"/>.
        /// </summary>
        public void Dispose() {
            if (isDisposed.Exchange(true))
                return;

            Scene.RemoveRenderCameraFromScene(this);
            Entity.Remove<CameraComponent>(Scene.EntityWorld);

            thread?.DisposeAndWait();
            thread = null;

            meshRenderer.Dispose();
            RenderTarget.Destroy();

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Renders one frame to <see cref="RenderTarget"/>.
        /// </summary>
        public void Render() {
            foreach (EntitySystemBase system in Scene.FrameDependentSystems)
                system.TryExecute();

            foreach (EntitySystemBase system in Scene.FrameDependentSystems)
                system.Wait();

            meshRenderer.ExecuteParallelAndWait();
        }

    }
}
