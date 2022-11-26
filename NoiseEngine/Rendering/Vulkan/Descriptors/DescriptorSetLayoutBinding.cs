using System;
using System.Runtime.InteropServices;

namespace NoiseEngine.Rendering.Vulkan.Descriptors;

/// <summary>
/// https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/VkDescriptorSetLayoutBinding.html
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal readonly record struct DescriptorSetLayoutBinding(
    uint Binding, DescriptorType Type, uint Count, ShaderStageFlags ShaderStageFlags, IntPtr ImmutableSamplers
);
