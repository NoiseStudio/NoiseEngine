using System;
using System.Runtime.InteropServices;

namespace NoiseEngine.Interop.InteropMarshalling;

[StructLayout(LayoutKind.Sequential)]
internal readonly unsafe ref struct InteropSpan<T> where T : unmanaged {

    private readonly T* reference;
    private readonly int length;

    public InteropSpan(T* reference, int length) {
        this.reference = reference;
        this.length = length;
    }

    public Span<T> AsSpan() {
        return new Span<T>(reference, length);
    }

}
