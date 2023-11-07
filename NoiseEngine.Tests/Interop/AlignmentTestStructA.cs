using System.Runtime.InteropServices;

namespace NoiseEngine.Tests.Interop;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct AlignmentTestStructA {

    private readonly byte a;
    private readonly double b;
    private readonly bool c;
    private readonly int d;

}
