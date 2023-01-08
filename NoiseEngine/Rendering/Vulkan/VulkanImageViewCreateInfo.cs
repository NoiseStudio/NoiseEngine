using NoiseEngine.Interop;
using System.Runtime.InteropServices;

namespace NoiseEngine.Rendering.Vulkan;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct VulkanImageViewCreateInfo(
    InteropHandle<Texture> ImageHandle,
    uint Flags,
    VulkanImageViewType Type,
    ComponentMapping Components,
    VulkanImageAspect AspectMask,
    uint BaseMipLevel,
    uint LevelCount,
    uint BaseArrayLayer,
    uint LayerCount
);
