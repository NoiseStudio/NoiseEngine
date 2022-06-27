using NoiseEngine.Jobs;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Presentation;
using NoiseEngine.Systems;
using System;
using System.Threading;

namespace NoiseEngine {
    internal class RenderCameraThread : IDisposable {

        private ManualResetEventSlim? disposedEvent;

        public bool IsDisposed { get; private set; }
        public ApplicationScene Scene { get; }
        public Camera Camera { get; }

        public Application Application => Scene.Application;
        public Window RenderTarget => Camera.RenderTarget;
        public bool IsWindow => true;
        public bool IsShouldClose => IsDisposed || (IsWindow && RenderTarget.GetShouldClose());

        internal RenderCameraThread(ApplicationScene scene, Camera camera, MeshRendererSystem meshRenderer) {
            Scene = scene;
            Camera = camera;

            new Thread(ThreadWorker) {
                Name = nameof(RenderCameraThread)
            }.Start(meshRenderer);
        }

        public void Dispose() {
            disposedEvent = new ManualResetEventSlim(false);
            IsDisposed = true;
        }

        public void DisposeAndWait() {
            Dispose();
            disposedEvent!.Wait();
        }

        private void ThreadWorker(object? meshRendererObject) {
            Application.IncrementActiveRenderers();

            MeshRendererSystem meshRenderer =
                (MeshRendererSystem)(meshRendererObject ?? throw new NullReferenceException());

            while (!IsShouldClose) {
                foreach (EntitySystemBase system in Scene.FrameDependentSystems)
                    system.TryExecute();

                foreach (EntitySystemBase system in Scene.FrameDependentSystems)
                    system.Wait();

                meshRenderer.ExecuteParallelAndWait();
            }

            disposedEvent?.Set();
            Application.DecrementActiveRenderers();
        }

    }
}
