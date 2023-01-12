using NoiseEngine.Interop;

namespace NoiseEngine.Rendering.Vulkan;

internal class SwapChain {

    public GraphicsDevice Device { get; }

    internal InteropHandle<RenderPass> Handle { get; }

    public SwapChain(GraphicsDevice device, Window window) {
        Device = device;
    }

    ~SwapChain() {

    }

}
