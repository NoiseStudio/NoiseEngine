using NoiseEngine.Rendering.Vulkan;

namespace NoiseEngine.Interop.Rendering.Vulkan;

internal static partial class ComputePipelineInterop {

    [InteropImport("rendering_vulkan_compute_pipeline_create")]
    public static partial InteropResult<InteropHandle<Pipeline>> Create(
        InteropHandle<PipelineLayout> layout, PipelineShaderStageRaw stage, PipelineCreateFlags flags
    );

}
