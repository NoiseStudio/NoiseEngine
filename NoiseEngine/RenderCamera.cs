using NoiseEngine.Components;
using NoiseEngine.Jobs;
using NoiseEngine.Systems;
using NoiseEngine.Threading;
using System;

namespace NoiseEngine;

public class RenderCamera : IDisposable {

    private readonly object locker = new object();
    private readonly MeshRendererSystem meshRenderer;

    private RenderCameraThread? thread;
    private AtomicBool isDisposed;

    public Entity Entity { get; }
    public ApplicationScene Scene { get; }
    public bool IsDisposed => isDisposed;

    public bool AutoRender {
        get => thread is not null;
        set {
            lock (locker) {
                if (AutoRender == value)
                    return;

                if (value) {
                    thread = new RenderCameraThread(this, meshRenderer);
                } else {
                    thread?.Dispose();
                    thread = null;
                }
            }
        }
    }

    /*public ProjectionType ProjectionType {
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

    internal Camera Camera { get; }*/

    internal RenderCamera(ApplicationScene scene, /*Camera camera, */bool autoRender) {
        Scene = scene;
        //Camera = camera;

        Entity = scene.EntityWorld.NewEntity(
            // TODO: implement
            new TransformComponent(/*camera.Position, camera.Rotation*/),
            new CameraComponent(this)
        );

        meshRenderer = new MeshRendererSystem(Scene.GraphicsDevice/*, Camera*/);
        meshRenderer.Initialize(Scene.EntityWorld, Application.EntitySchedule);

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
        //RenderTarget.Destroy();

        // Auto exit when all windows are closed.
        // TODO: move this to Window class.
        if (Application.Settings.AutoExitWhenAllWindowsAreClosed /*&& Application.Windows.Any()*/)
            Application.Exit();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Renders one frame to render target.
    /// </summary>
    public void Render() {
        foreach (EntitySystemBase system in Scene.FrameDependentSystems)
            system.TryExecute();

        foreach (EntitySystemBase system in Scene.FrameDependentSystems)
            system.Wait();

        meshRenderer.ExecuteParallelAndWait();
    }

}
