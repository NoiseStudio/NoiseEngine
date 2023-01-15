using NoiseEngine.Rendering;
using System;

namespace NoiseEngine;

public class Camera : SimpleCamera {

    private readonly object renderLoopLocker = new object();
    private RenderLoop? renderLoop;

    public ApplicationScene Scene { get; }

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
                renderLoop = value;
                renderLoop?.InternalInitialize(this);
            }
        }
    }

    public Camera(ApplicationScene scene) : base(scene.GraphicsDevice) {
        Scene = scene;
    }

    private protected override void RaiseRenderTargetSet(ICameraRenderTarget? newRenderTarget) {
        lock (renderLoopLocker) {
            if (renderLoop is not null && newRenderTarget is not Window) {
                renderLoop.InternalDeinitialize();
                renderLoop = null;
            }
        }
    }

}
