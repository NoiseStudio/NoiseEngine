using System.Runtime.InteropServices;

namespace NoiseEngine.Rendering.Vulkan;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct VulkanImageCreateInfo(
    uint Flags,
    VulkanImageType Type,
    VulkanImageLayout Layout
);
