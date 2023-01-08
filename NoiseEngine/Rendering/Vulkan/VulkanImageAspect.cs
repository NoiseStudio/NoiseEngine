using System;

namespace NoiseEngine.Rendering.Vulkan;

/// <summary>
/// https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/VkImageAspectFlagBits.html
/// </summary>
[Flags]
internal enum VulkanImageAspect : uint {
    Color = 1 << 0,
    Depth = 1 << 1,
    Stencil = 1 << 2,
    Metadata = 1 << 3
}
