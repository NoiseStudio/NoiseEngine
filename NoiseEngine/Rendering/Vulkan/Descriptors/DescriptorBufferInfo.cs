using NoiseEngine.Rendering.Buffers;
using System;

namespace NoiseEngine.Rendering.Vulkan.Descriptors;

internal readonly record struct DescriptorBufferInfo(IntPtr Buffer, ulong Offset, ulong Range) {

    private const ulong WholeSize = ~0ul;

    public static DescriptorBufferInfo Create(GraphicsReadOnlyBuffer buffer) {
        return new DescriptorBufferInfo(buffer.InnerHandleUniversal.Pointer, 0, WholeSize);
    }

}
