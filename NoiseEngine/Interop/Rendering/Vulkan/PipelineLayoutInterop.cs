using NoiseEngine.Rendering.Vulkan;
using NoiseEngine.Rendering.Vulkan.Descriptors;
using System;

namespace NoiseEngine.Interop.Rendering.Vulkan;

internal static partial class PipelineLayoutInterop {

    [InteropImport("rendering_vulkan_pipeline_layout_create")]
    public static partial InteropResult<InteropHandle<PipelineLayout>> Create(
        ReadOnlySpan<InteropHandle<DescriptorSetLayout>> layouts, ReadOnlySpan<PushConstantRange> pushConstantRanges
    );

    [InteropImport("rendering_vulkan_pipeline_layout_destroy")]
    public static partial void Destroy(InteropHandle<PipelineLayout> handle);

}
