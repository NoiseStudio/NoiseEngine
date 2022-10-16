using NoiseEngine.Rendering;

namespace NoiseEngine.Interop.Graphics.Vulkan;

internal static partial class VulkanDeviceInterop {

    [InteropImport("graphics_vulkan_device_interop_destroy")]
    public static partial void Destroy(InteropHandle<GraphicsDevice> device);

    [InteropImport("graphics_vulkan_device_interop_initialize")]
    public static partial InteropResult<None> Initialize(InteropHandle<GraphicsDevice> device);

}
