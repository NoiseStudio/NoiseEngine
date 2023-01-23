namespace NoiseEngine.Rendering;

internal abstract class SimpleCameraDelegation {

    public SimpleCamera Camera { get; }
    public GraphicsDevice GraphicsDevice => Camera.GraphicsDevice;

    protected SimpleCameraDelegation(SimpleCamera camera) {
        Camera = camera;
    }

    public abstract void UpdateClearColor();
    public abstract void ClearRenderTarget();

    public abstract uint ChangeFramesInFlightCount(uint targetFramesInFlightCount);

}
