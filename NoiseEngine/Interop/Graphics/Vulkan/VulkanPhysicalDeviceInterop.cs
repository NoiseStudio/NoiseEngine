using NoiseEngine.Rendering;

namespace NoiseEngine.Interop.Graphics.Vulkan;

internal partial class VulkanPhysicalDeviceInterop {

    [InteropImport("graphics_vulkan_physical_device_interop_destroy")]
    public static partial void Destroy(InteropHandle<GraphicsPhysicalDevice> handle);

}
