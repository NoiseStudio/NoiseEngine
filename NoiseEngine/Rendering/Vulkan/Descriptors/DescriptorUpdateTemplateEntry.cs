using System.Runtime.InteropServices;

namespace NoiseEngine.Rendering.Vulkan.Descriptors;

/// <summary>
/// https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/VkDescriptorUpdateTemplateEntry.html
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal readonly record struct DescriptorUpdateTemplateEntry(
    uint DestinationBinding, uint DestinationArrayElement, uint Count, DescriptorType Type, nuint Offset, nuint Stride
);
