using System;

namespace NoiseEngine.Interop.Graphics.Buffers;

internal static partial class GraphicsBufferInterop {

    [InteropImport("graphics_buffers_buffer_interop_destroy")]
    public static partial void Destroy(IntPtr handle);

    [InteropImport("graphics_buffers_buffer_interop_host_read")]
    public static partial InteropResult<None> HostRead(IntPtr buffer, Span<byte> destinationBuffer, ulong start);

    [InteropImport("graphics_buffers_buffer_interop_host_write")]
    public static partial InteropResult<None> HostWrite(IntPtr buffer, ReadOnlySpan<byte> data, ulong start);

}
