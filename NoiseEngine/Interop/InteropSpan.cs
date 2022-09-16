using System;
using System.Runtime.InteropServices;

namespace NoiseEngine.Interop;

[StructLayout(LayoutKind.Sequential)]
internal readonly unsafe ref struct InteropSpan<T> where T : unmanaged {

    private readonly void* reference;
    private readonly int length;

    public InteropSpan(Span<T> span) {
        fixed (void* reference = &MemoryMarshal.GetReference(span))
            this.reference = reference;
        length = span.Length;
    }

    public static implicit operator InteropSpan<T>(Span<T> span) {
        return new InteropSpan<T>(span);
    }

    public static implicit operator Span<T>(InteropSpan<T> span) {
        return new Span<T>(span.reference, span.length);
    }

    public static implicit operator ReadOnlySpan<T>(InteropSpan<T> span) {
        return new ReadOnlySpan<T>(span.reference, span.length);
    }

    public static implicit operator InteropReadOnlySpan<T>(InteropSpan<T> span) {
        return new InteropReadOnlySpan<T>(span.reference, span.length);
    }

}
