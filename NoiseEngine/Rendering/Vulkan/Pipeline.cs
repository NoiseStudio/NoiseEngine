using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering.Vulkan;
using System.Collections.Generic;

namespace NoiseEngine.Rendering.Vulkan;

internal abstract class Pipeline {

    public PipelineLayout Layout { get; }
    public IReadOnlyList<PipelineShaderStage> Stages { get; }

    internal InteropHandle<Pipeline> Handle { get; }

    protected Pipeline(
        PipelineLayout layout, IReadOnlyList<PipelineShaderStage> stages, InteropHandle<Pipeline> handle
    ) {
        Layout = layout;
        Stages = stages;
        Handle = handle;
    }

    ~Pipeline() {
        if (Handle.IsNull)
            return;

        PipelineInterop.Destroy(Handle);
    }

}
