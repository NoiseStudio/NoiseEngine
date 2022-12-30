using NoiseEngine.Rendering.Vulkan;
using System.Runtime.InteropServices;

namespace NoiseEngine.Interop.Rendering.Vulkan;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct VulkanInstanceCreateReturnValue(
    InteropHandle<VulkanInstance> Handle, InteropHandle<VulkanInstance> InnerHandle
);
