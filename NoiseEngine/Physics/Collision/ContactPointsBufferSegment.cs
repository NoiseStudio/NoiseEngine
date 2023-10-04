using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NoiseEngine.Physics.Collision;

internal class ContactPointsBufferSegment<T> {

    private readonly ContactWithPointer<T>[] buffer;
    private ContactPointsBufferSegment<T>? next;

    public int Size { get; }
    public ContactPointsBufferSegment<T>? Next => next;

    public ref ContactWithPointer<T> this[int index] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            ContactPointsBufferSegment<T>? segment = this;

            while (index >= segment.Size) {
                index -= segment.Size;
                if (segment.Next is null)
                    segment.CreateNext();
                segment = segment.Next!;
            }

            return ref segment.buffer[index];
        }
    }

    public ContactPointsBufferSegment(int size) {
        buffer = new ContactWithPointer<T>[size];
        Size = size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetNextSize(int size) {
        return size * 2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ContactPointsBufferSpan<T> AsSpan(int start) {
        ContactPointsBufferSegment<T>? segment = this;

        while (start >= segment.Size) {
            start -= segment.Size;
            segment = segment.Next;

            if (segment is null)
                throw new UnreachableException();
        }

        return new ContactPointsBufferSpan<T>(segment, start);
    }

    public Span<ContactWithPointer<T>> GetData() {
        return buffer;
    }

    public Span<ContactWithPointer<T>> GetData(int start) {
        return buffer.AsSpan(start);
    }

    public void CreateNext() {
        Interlocked.CompareExchange(ref next, new ContactPointsBufferSegment<T>(GetNextSize(Size)), null);
    }

    public void Clear(int used) {
        Array.Clear(buffer, 0, used);
    }

}
