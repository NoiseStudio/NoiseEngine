using NoiseEngine.Rendering.Buffers;
using System;
using System.Runtime.InteropServices;

namespace NoiseEngine.Rendering.Vulkan.Descriptors;

/// <summary>
/// https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/VkDescriptorBufferInfo.html
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal readonly record struct DescriptorBufferInfo(IntPtr Buffer, ulong Offset, ulong Range) {

    private const ulong WholeSize = ~0ul;

    public static DescriptorBufferInfo Create(GraphicsReadOnlyBuffer buffer) {
        return new DescriptorBufferInfo(buffer.InnerHandleUniversal.Pointer, 0, WholeSize);
    }

}
