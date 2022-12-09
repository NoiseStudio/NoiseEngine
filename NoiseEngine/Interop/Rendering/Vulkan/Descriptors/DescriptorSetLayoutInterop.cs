using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Vulkan.Descriptors;
using System;

namespace NoiseEngine.Interop.Rendering.Vulkan.Descriptors;

internal static partial class DescriptorSetLayoutInterop {

    [InteropImport("rendering_vulkan_descriptors_set_layout_create")]
    public static partial InteropResult<InteropHandle<DescriptorSetLayout>> Create(
        InteropHandle<GraphicsDevice> device, uint flags, ReadOnlySpan<DescriptorSetLayoutBinding> bindings
    );

    [InteropImport("rendering_vulkan_descriptors_set_layout_destroy")]
    public static partial void Destroy(InteropHandle<DescriptorSetLayout> handle);

}
