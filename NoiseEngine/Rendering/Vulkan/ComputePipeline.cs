using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering.Vulkan;

namespace NoiseEngine.Rendering.Vulkan;

internal class ComputePipeline : Pipeline {

    public ComputePipeline(
        PipelineLayout layout, PipelineShaderStage stage, PipelineCreateFlags flags
    ) : base(layout, new PipelineShaderStage[] { stage }, CreateHandle(layout, stage, flags)) {
    }

    private static InteropHandle<Pipeline> CreateHandle(
        PipelineLayout layout, PipelineShaderStage stage, PipelineCreateFlags flags
    ) {
        if (!ComputePipelineInterop.Create(layout.Handle, new PipelineShaderStageRaw(stage), flags).TryGetValue(
            out InteropHandle<Pipeline> handle, out ResultError error
        )) {
            error.ThrowAndDispose();
        }

        return handle;
    }

}
