using NoiseEngine.Interop;

namespace NoiseEngine.Rendering.Buffers;

public abstract class GraphicsReadOnlyBuffer {

    public GraphicsDevice Device { get; }
    public ulong Count { get; }

    internal abstract InteropHandle<GraphicsReadOnlyBuffer> HandleUniversal { get; }
    internal abstract InteropHandle<GraphicsReadOnlyBuffer> InnerHandleUniversal { get; }

    private protected GraphicsReadOnlyBuffer(GraphicsDevice device, ulong count) {
        Device = device;
        Count = count;
    }

}
