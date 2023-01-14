using NoiseEngine.Interop.InteropMarshalling;
using NoiseEngine.Rendering.Vulkan;
using System;

namespace NoiseEngine.Interop.Rendering.Vulkan;

internal static partial class VulkanInstanceInterop {

    [InteropImport("rendering_vulkan_instance_interop_create")]
    public static partial InteropResult<VulkanInstanceCreateReturnValue> Create(
        InteropHandle<VulkanLibrary> library, VulkanApplicationInfo createInfo, VulkanLogSeverity logSeverity,
        VulkanLogType logType, bool validation, ReadOnlySpan<InteropString> enabledExtensions
    );

    [InteropImport("rendering_vulkan_instance_interop_destroy")]
    public static partial void Destroy(InteropHandle<VulkanInstance> instance);

    [InteropImport("rendering_vulkan_instance_interop_get_devices")]
    public static partial InteropResult<InteropArray<VulkanDeviceValue>> GetDevices(
        InteropHandle<VulkanInstance> instance
    );

}
