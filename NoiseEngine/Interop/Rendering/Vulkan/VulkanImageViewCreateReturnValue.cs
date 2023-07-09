using NoiseEngine.Rendering.Vulkan;
using System.Runtime.InteropServices;

namespace NoiseEngine.Interop.Rendering.Vulkan;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct VulkanImageViewCreateReturnValue(
    InteropHandle<VulkanImageView> Handle, InteropHandle<VulkanImageView> InnerHandle
);
