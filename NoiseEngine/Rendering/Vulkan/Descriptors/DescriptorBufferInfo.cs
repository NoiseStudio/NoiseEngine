using NoiseEngine.Rendering.Buffers;
using System;

namespace NoiseEngine.Rendering.Vulkan.Descriptors;

internal readonly record struct DescriptorBufferInfo(IntPtr Buffer, ulong Offset, ulong Range) {

    private const ulong WholeSize = ~0ul;

    public static DescriptorBufferInfo Create<T>(GraphicsReadOnlyBuffer<T> buffer) where T : unmanaged {
        return new DescriptorBufferInfo(buffer.InnerHandle.Pointer, 0, WholeSize);
    }

}
