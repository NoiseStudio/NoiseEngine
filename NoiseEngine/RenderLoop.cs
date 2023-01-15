namespace NoiseEngine;

public abstract class RenderLoop {

    public Camera? Camera { get; set; }
    public Window? Window { get; set; }

    /// <summary>Returns a string that represents the current <see cref="RenderLoop"/>.</summary>
    /// <returns>A string that represents the current <see cref="RenderLoop"/>.</returns>
    public override string ToString() {
        Window? window = Window;
        if (window is null)
            return nameof(RenderLoop);
        return $"{nameof(RenderLoop)} on Window.Id = {window.Id}";
    }

    internal void InternalInitialize(Camera camera) {
        Camera = camera;
        Window = (Window)Camera.RenderTarget!;

        Initialize();
    }

    internal void InternalDeinitialize() {
        Deinitialize();
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
