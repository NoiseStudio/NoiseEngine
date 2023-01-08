using NoiseEngine.Rendering;

namespace NoiseEngine.Interop.Rendering.Vulkan;

internal static partial class VulkanImageInterop {

    [InteropImport("rendering_vulkan_image_interop_create")]
    public static partial InteropResult<TextureCreateReturnValue> Create(
        InteropHandle<GraphicsDevice> device, VulkanImageCreateInfoRaw createInfo
    );

}
