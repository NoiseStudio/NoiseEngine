using NoiseEngine.Rendering.Vulkan.Descriptors;
using System;

namespace NoiseEngine.Interop.Rendering.Vulkan.Descriptors;

internal static partial class DescriptorSetInterop {

    [InteropImport("rendering_vulkan_descriptors_set_create")]
    public static partial InteropResult<InteropHandle<DescriptorSet>> Create(InteropHandle<DescriptorSetLayout> layout);

    [InteropImport("rendering_vulkan_descriptors_set_destroy")]
    public static partial void Destroy(InteropHandle<DescriptorSet> handle);

    [InteropImport("rendering_vulkan_descriptors_set_update")]
    public static partial void Update(
        InteropHandle<DescriptorSet> handle, InteropHandle<DescriptorUpdateTemplate> template,
        ReadOnlySpan<byte> data
    );

}
