using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering.Vulkan;

namespace NoiseEngine.Rendering.Vulkan;

internal abstract class Pipeline {

    public PipelineLayout Layout { get; }
    public PipelineShaderStage Stage { get; }

    internal InteropHandle<Pipeline> Handle { get; }

    protected Pipeline(PipelineLayout layout, PipelineShaderStage stage, InteropHandle<Pipeline> handle) {
        Layout = layout;
        Stage = stage;
        Handle = handle;
    }

    ~Pipeline() {
        if (Handle.IsNull)
            return;

        PipelineInterop.Destroy(Handle);
    }

}
