using NoiseEngine.Rendering.Vulkan;
using System;

namespace NoiseEngine.Interop.Rendering.Vulkan;

internal static partial class GraphicsPipelineInterop {

    [InteropImport("rendering_vulkan_graphics_pipeline_create")]
    public static partial InteropResult<InteropHandle<Pipeline>> Create(
        InteropHandle<RenderPass> renderPass, InteropHandle<PipelineLayout> layout,
        ReadOnlySpan<PipelineShaderStageRaw> stages, PipelineCreateFlags flags, GraphicsPipelineCreateInfoRaw createInfo
    );

}
