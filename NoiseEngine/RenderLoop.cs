using NoiseEngine.Common;
using System;
using System.Threading;

namespace NoiseEngine;

public abstract class RenderLoop {

    private readonly object deinitializeLocker = new object();

    private Camera? camera;

    public Camera? Camera => camera;
    public Window? Window { get; private set; }

    /// <summary>
    /// Returns a string that represents the current <see cref="RenderLoop"/>.
    /// </summary>
    /// <returns>A string that represents the current <see cref="RenderLoop"/>.</returns>
    public override string ToString() {
        Window? window = Window;
        if (window is null)
            return nameof(RenderLoop);
        return $"{nameof(RenderLoop)} on Window.Id = {window.Id}";
    }

    internal void InternalInitialize(Camera camera) {
        Camera? exchanged = Interlocked.CompareExchange(ref this.camera, camera, null);
        if (exchanged != null)
            throw new InvalidOperationException($"This {nameof(RenderLoop)} is currently assigned to the {exchanged}.");

        ((IReferenceCoutable)camera.RenderTarget!).RcRetain();

        this.camera = camera;
        Window = (Window)camera.RenderTarget!;

        Initialize();
    }

    internal void InternalDeinitialize() {
        lock (deinitializeLocker)
            Deinitialize();

        camera = null;
        Window oldWindow = Window!;
        Window = null;

        ((IReferenceCoutable)oldWindow).RcRelease();
    }

    /// <summary>
    /// Calls on <see cref="RenderLoop"/> initialization.
    /// </summary>
    protected abstract void Initialize();

    /// <summary>
    /// Calls on <see cref="RenderLoop"/> is deinitialization.
    /// </summary>
    protected abstract void Deinitialize();

}
