using NoiseEngine.Rendering;

namespace NoiseEngine;

internal abstract class CameraDelegation {

    public Camera Camera { get; }
    public GraphicsDevice GraphicsDevice => Camera.GraphicsDevice;

    protected CameraDelegation(Camera camera) {
        Camera = camera;
    }

    public abstract void UpdateClearColor();
    public abstract void ClearRenderTarget();

}
