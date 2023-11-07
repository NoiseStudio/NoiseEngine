using System.Runtime.InteropServices;

namespace NoiseEngine.Interop;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct MemoryAlignmentHelper<T> where T : unmanaged {

    public readonly byte Padding;
    public readonly T Value;

}
