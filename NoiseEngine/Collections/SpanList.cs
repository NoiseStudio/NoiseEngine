using System;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Collections;

internal ref struct SpanList<T> {

    internal readonly Span<T> buffer;

    public readonly Span<T> Data => buffer[..Count];
    public int Count { readonly get; set; }

    public SpanList(Span<T> buffer, int initCount) {
        this.buffer = buffer;
        Count = initCount;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item) {
        buffer[Count++] = item;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveAt(int index) {
        Span<T> data = Data;
        data[(index + 1)..].CopyTo(data[index..]);
        Count--;
    }

}
