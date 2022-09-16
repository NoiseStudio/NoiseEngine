using System;
using System.Runtime.InteropServices;

namespace NoiseEngine.Interop;

[StructLayout(LayoutKind.Sequential)]
internal readonly unsafe ref struct InteropReadOnlySpan<T> where T : unmanaged {

    private readonly T* reference;
    private readonly int length;

    public InteropReadOnlySpan(T* reference, int length) {
        this.reference = reference;
        this.length = length;
    }

    public InteropReadOnlySpan(ReadOnlySpan<T> span) {
        fixed (T* reference = &MemoryMarshal.GetReference(span))
            this.reference = reference;
        length = span.Length;
    }

    public static implicit operator InteropReadOnlySpan<T>(ReadOnlySpan<T> span) {
        return new InteropReadOnlySpan<T>(span);
    }

    public static implicit operator InteropReadOnlySpan<T>(Span<T> span) {
        return new InteropReadOnlySpan<T>(span);
    }

    public static implicit operator ReadOnlySpan<T>(InteropReadOnlySpan<T> span) {
        return new ReadOnlySpan<T>(span.reference, span.length);
    }

}
