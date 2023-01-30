using NoiseEngine.Components;
using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using NoiseEngine.Rendering;
using NoiseEngine.Systems;
using System;
using System.Threading;

namespace NoiseEngine;

public class Camera : SimpleCamera {

    private readonly object renderLoopLocker = new object();
    private readonly object renderFrameResourcesLock = new object();

    private RenderLoop? renderLoop;
    private RenderFrameResources? renderFrameResources;
    private MeshRendererSystem? meshRendererSystem;

    public ApplicationScene Scene { get; }
    public Entity Entity { get; }

    /// <summary>
    /// Assigns given <see cref="RenderLoop"/> to this <see cref="Camera"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// RenderTarget is not <see cref="Window"/> and render loop is not <see langword="null"/>.
    /// </exception>
    public RenderLoop? RenderLoop {
        get => renderLoop;
        set {
            lock (renderLoopLocker) {
                if (value is not null && RenderTarget is not Window window) {
                    throw new InvalidOperationException(
                        $"{nameof(RenderTarget)} is not {nameof(Window)} and render queue is not null."
                    );
                }

                renderLoop?.InternalDeinitialize();
                value?.InternalInitialize(this);
                renderLoop = value;
            }
        }
    }

    public Camera(
        ApplicationScene scene, Vector3<float> position, Quaternion<float> rotation
    ) : base(scene.GraphicsDevice) {
        Scene = scene;
        Entity = scene.EntityWorld.NewEntity(
            new TransformComponent(position, rotation),
            new CameraComponent(this)
        );

        Scene.AddCameraToScene(this);
    }

    public Camera(ApplicationScene scene, Vector3<float> position) : this(scene, position, Quaternion<float>.Identity) {
    }

    public Camera(ApplicationScene scene) : this(scene, new Vector3<float>(0, 0, -5)) {
    }

    /// <summary>
    /// Renders frame manually.
    /// </summary>
    public void Render() {
        Window? window = null;
        if (RenderTarget is Window w) {
            window = w;
            Monitor.Enter(window.PoolEventsLocker);
            window.PoolEvents();
        }

        lock (renderFrameResourcesLock) {
            try {
                if (renderFrameResources is null) {
                    renderFrameResources = new RenderFrameResources(GraphicsDevice, this);
                    meshRendererSystem = new MeshRendererSystem(this);
                    meshRendererSystem.Initialize(Scene.EntityWorld, Application.EntitySchedule);
                    meshRendererSystem.Resources = renderFrameResources.MeshRendererResources;
                }

                foreach (EntitySystemBase system in Scene.FrameDependentSystems)
                    system.TryExecute();

                foreach (EntitySystemBase system in Scene.FrameDependentSystems)
                    system.Wait();
            } finally {
                if (window is not null)
                    Monitor.Exit(window.PoolEventsLocker);
            }

            TransformComponent transform = Entity.Get<TransformComponent>(Scene.EntityWorld);
            Position = transform.Position;
            Rotation = transform.Rotation;
            meshRendererSystem!.ExecuteParallelAndWait();

            renderFrameResources.RecordAndExecute();
            renderFrameResources.Clear();
        }
    }

    private protected override void RaiseRenderTargetSet(ICameraRenderTarget? newRenderTarget) {
        lock (renderLoopLocker) {
            if (renderLoop is not null && newRenderTarget is not Window) {
                RenderLoop loop = renderLoop;
                renderLoop = null;

                loop.InternalDeinitialize();
            }
        }
    }

}
