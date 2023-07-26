using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NoiseEngine.Physics.Collision;

internal class ContactPointsBufferSegment {

    private readonly ContactPointWithPointer[] buffer;
    private ContactPointsBufferSegment? next;

    public int Size { get; }
    public ContactPointsBufferSegment? Next => next;

    public ref ContactPointWithPointer this[int index] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            ContactPointsBufferSegment? segment = this;

            while (index >= segment.Size) {
                index -= segment.Size;
                segment = segment.Next ?? throw new UnreachableException();
            }

            return ref segment.buffer[index];
        }
    }

    public ContactPointsBufferSegment(int size) {
        buffer = new ContactPointWithPointer[size];
        Size = size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetNextSize(int size) {
        return size * 2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ContactPointsBufferSpan AsSpan(int start) {
        ContactPointsBufferSegment? segment = this;

        while (start >= segment.Size) {
            start -= segment.Size;
            segment = segment.Next;

            if (segment is null)
                throw new UnreachableException();
        }

        return new ContactPointsBufferSpan(segment, start);
    }

    public Span<ContactPointWithPointer> GetData() {
        return buffer;
    }

    public Span<ContactPointWithPointer> GetData(int start) {
        return buffer.AsSpan(start);
    }

    public void CreateNext() {
        Interlocked.CompareExchange(ref next, new ContactPointsBufferSegment(GetNextSize(Size)), null);
    }

    public void Clear(int used) {
        Array.Clear(buffer, 0, used);
    }

}
