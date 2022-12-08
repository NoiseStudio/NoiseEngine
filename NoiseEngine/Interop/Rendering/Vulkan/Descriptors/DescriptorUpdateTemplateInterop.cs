using NoiseEngine.Rendering.Vulkan.Descriptors;
using System;

namespace NoiseEngine.Interop.Rendering.Vulkan.Descriptors;

internal static partial class DescriptorUpdateTemplateInterop {

    [InteropImport("rendering_vulkan_descriptors_update_template_create")]
    public static partial InteropResult<InteropHandle<DescriptorUpdateTemplate>> Create(
        InteropHandle<DescriptorSetLayout> layout, ReadOnlySpan<DescriptorUpdateTemplateEntry> entries
    );

    [InteropImport("rendering_vulkan_descriptors_update_template_destroy")]
    public static partial void Destroy(InteropHandle<DescriptorUpdateTemplate> handle);

}
