using NoiseEngine.Jobs;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Presentation;
using NoiseEngine.Systems;
using System;
using System.Threading;

namespace NoiseEngine;

internal class RenderCameraThread : IDisposable {

    private ManualResetEventSlim? disposedEvent;
    private WeakReference<RenderCamera> renderCamera;

    public bool IsDisposed { get; private set; }
    public ApplicationScene Scene { get; }
    public Camera Camera { get; }

    public Window RenderTarget => Camera.RenderTarget;
    public bool IsWindow => true;
    public bool IsShouldClose => IsDisposed || (IsWindow && RenderTarget.GetShouldClose());

    internal RenderCameraThread(RenderCamera renderCamera, MeshRendererSystem meshRenderer) {
        this.renderCamera = new WeakReference<RenderCamera>(renderCamera);

        Scene = renderCamera.Scene;
        Camera = renderCamera.Camera;

        new Thread(ThreadWorker) {
            Name = nameof(RenderCameraThread)
        }.Start(meshRenderer);
    }

    public void Dispose() {
        disposedEvent ??= new ManualResetEventSlim(false);
        IsDisposed = true;
    }

    public void DisposeAndWait() {
        Dispose();
        disposedEvent!.Wait();
    }

    private void ThreadWorker(object? meshRendererObject) {
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

        // Dispose render camera when windows is closed.
        // TODO: move this to Window class as event.
        if (RenderTarget.GetShouldClose() && renderCamera.TryGetTarget(out RenderCamera? r)) {
            disposedEvent = new ManualResetEventSlim(true);
            r.Dispose();
        }
    }

}
