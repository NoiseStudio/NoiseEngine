using System;
using System.Runtime.InteropServices;

namespace NoiseEngine.Interop.InteropMarshalling;

[StructLayout(LayoutKind.Sequential)]
internal readonly unsafe ref struct InteropReadOnlySpan<T> where T : unmanaged {

    private readonly T* reference;
    private readonly int length;

    public InteropReadOnlySpan(T* reference, int length) {
        this.reference = reference;
        this.length = length;
    }

    public ReadOnlySpan<T> AsSpan() {
        return new ReadOnlySpan<T>(reference, length);
    }

}
