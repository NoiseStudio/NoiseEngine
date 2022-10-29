using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Buffers;
using System;

namespace NoiseEngine.Interop.Rendering.Vulkan.Buffers;

internal static partial class VulkanBufferInterop {

    [InteropImport("rendering_vulkan_buffers_buffer_interop_create")]
    public static partial InteropResult<IntPtr> Create(
        InteropHandle<GraphicsDevice> device, GraphicsBufferUsage usage, ulong size, bool map
    );

}
