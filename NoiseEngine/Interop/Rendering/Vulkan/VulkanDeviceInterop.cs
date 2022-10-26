using NoiseEngine.Rendering;

namespace NoiseEngine.Interop.Rendering.Vulkan;

internal static partial class VulkanDeviceInterop {

    [InteropImport("rendering_vulkan_device_interop_destroy")]
    public static partial void Destroy(InteropHandle<GraphicsDevice> device);

    [InteropImport("rendering_vulkan_device_interop_initialize")]
    public static partial InteropResult<None> Initialize(InteropHandle<GraphicsDevice> device);

}
