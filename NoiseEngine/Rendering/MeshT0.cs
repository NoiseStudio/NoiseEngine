using NoiseEngine.Rendering.Buffers;

namespace NoiseEngine.Rendering;

public abstract class Mesh {

    public GraphicsDevice Device { get; set; }

    private protected Mesh(GraphicsDevice device) {
        Device = device;
    }

    internal abstract (GraphicsReadOnlyBuffer vertexBuffer, GraphicsReadOnlyBuffer indexBuffer) GetBuffers();

}
