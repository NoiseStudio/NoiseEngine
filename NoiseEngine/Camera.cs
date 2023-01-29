using NoiseEngine.Components;
using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Buffers;
using System;

namespace NoiseEngine;

public class Camera : SimpleCamera {

    private readonly object renderLoopLocker = new object();
    private RenderLoop? renderLoop;

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
    }

    public Camera(ApplicationScene scene, Vector3<float> position) : this(scene, position, Quaternion<float>.Identity) {
    }

    public Camera(ApplicationScene scene) : this(scene, new Vector3<float>(0, 0, -5)) {
    }

    /// <summary>
    /// Renders frame manually.
    /// </summary>
    public void Render() {
        if (RenderTarget is Window window) {
            lock (window.PoolEventsLocker)
                window.PoolEvents();
        }

        GraphicsCommandBuffer commandBuffer = new GraphicsCommandBuffer(GraphicsDevice, false);

        commandBuffer.AttachCameraUnchecked(this);
        commandBuffer.DetachCameraUnchecked();

        commandBuffer.Execute();
        commandBuffer.Clear();
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
