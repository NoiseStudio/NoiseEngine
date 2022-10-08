using NoiseEngine.Rendering.Vulkan;

namespace NoiseEngine.Interop.Graphics.Vulkan;

internal partial class VulkanInstanceInterop {

    [InteropImport("graphics_vulkan_vulkan_instance_interop_create")]
    public static partial InteropResult<InteropHandle<VulkanInstance>> Create(
        InteropHandle<VulkanLibrary> library, VulkanInstanceCreateInfo createInfo,
        VulkanLogSeverity logSeverity, VulkanLogType logType
    );

    [InteropImport("graphics_vulkan_vulkan_instance_interop_destroy")]
    public static partial void Destroy(InteropHandle<VulkanInstance> instance);

}
