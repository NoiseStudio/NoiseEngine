using NoiseEngine.Rendering.Vulkan;

namespace NoiseEngine.Interop.Rendering.Vulkan;

internal static partial class VulkanImageViewInterop {

    [InteropImport("rendering_vulkan_image_view_interop_create")]
    public static partial InteropResult<VulkanImageViewCreateReturnValue> Create(VulkanImageViewCreateInfo createInfo);

    [InteropImport("rendering_vulkan_image_view_interop_destroy")]
    public static partial void Destroy(InteropHandle<VulkanImageView> imageView);

}
