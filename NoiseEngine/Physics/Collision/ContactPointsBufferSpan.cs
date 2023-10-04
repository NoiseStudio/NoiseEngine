using System;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics.Collision;

internal readonly ref struct ContactPointsBufferSpan<T> {

    private readonly Span<ContactWithPointer<T>> first;
    private readonly ContactPointsBufferSegment<T> segment;

    public ContactPointsBufferSpan(ContactPointsBufferSegment<T> segment, int segmentStart) {
        first = segment.GetData(segmentStart);
        this.segment = segment;
    }

    public ref ContactWithPointer<T> this[int index] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            Span<ContactWithPointer<T>> span = first;
            ContactPointsBufferSegment<T>? segment = this.segment;

            while (index >= span.Length) {
                index -= span.Length;
                span = segment.GetData();

                if (segment.Next is null)
                    segment.CreateNext();
                segment = segment.Next!;
            }

            return ref span[index];
        }
    }

}
