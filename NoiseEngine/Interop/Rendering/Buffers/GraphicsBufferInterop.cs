using System;

namespace NoiseEngine.Interop.Rendering.Buffers;

internal static partial class GraphicsBufferInterop {

    [InteropImport("rendering_buffers_buffer_interop_destroy")]
    public static partial void Destroy(IntPtr handle);

    [InteropImport("rendering_buffers_buffer_interop_host_read")]
    public static partial InteropResult<None> HostRead(IntPtr buffer, Span<byte> destinationBuffer, ulong start);

    [InteropImport("rendering_buffers_buffer_interop_host_write")]
    public static partial InteropResult<None> HostWrite(IntPtr buffer, ReadOnlySpan<byte> data, ulong start);

}
