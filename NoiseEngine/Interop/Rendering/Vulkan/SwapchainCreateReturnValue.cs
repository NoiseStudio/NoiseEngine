using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Vulkan;
using System.Runtime.InteropServices;

namespace NoiseEngine.Interop.Rendering.Vulkan;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct SwapchainCreateReturnValue(
    InteropHandle<Swapchain> Handle, TextureFormat Format
);
