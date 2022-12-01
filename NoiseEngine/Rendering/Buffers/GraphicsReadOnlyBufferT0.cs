using NoiseEngine.Interop;

namespace NoiseEngine.Rendering.Buffers;

public abstract class GraphicsReadOnlyBuffer {

    internal abstract InteropHandle<GraphicsReadOnlyBuffer> HandleUniversal { get; }
    internal abstract InteropHandle<GraphicsReadOnlyBuffer> InnerHandleUniversal { get; }

}
