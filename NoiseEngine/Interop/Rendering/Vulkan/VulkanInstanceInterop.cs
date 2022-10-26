using NoiseEngine.Rendering.Vulkan;

namespace NoiseEngine.Interop.Rendering.Vulkan;

internal static partial class VulkanInstanceInterop {

    [InteropImport("rendering_vulkan_instance_interop_create")]
    public static partial InteropResult<InteropHandle<VulkanInstance>> Create(
        InteropHandle<VulkanLibrary> library, VulkanInstanceCreateInfo createInfo,
        VulkanLogSeverity logSeverity, VulkanLogType logType
    );

    [InteropImport("rendering_vulkan_instance_interop_destroy")]
    public static partial void Destroy(InteropHandle<VulkanInstance> instance);

    [InteropImport("rendering_vulkan_instance_interop_get_devices")]
    public static partial InteropResult<InteropArray<VulkanDeviceValue>> GetDevices(
        InteropHandle<VulkanInstance> instance
    );

}
