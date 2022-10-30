using NoiseEngine.Interop.InteropMarshalling;
using System.Runtime.InteropServices;

namespace NoiseEngine.Interop.Rendering.Buffers;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct GraphicsCommandBufferUsage(
    InteropBool Graphics, InteropBool Computing, InteropBool Transfer
);
