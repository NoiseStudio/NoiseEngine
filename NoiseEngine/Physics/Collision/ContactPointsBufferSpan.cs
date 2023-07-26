using System;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics.Collision;

internal readonly ref struct ContactPointsBufferSpan {

    private readonly Span<ContactPointWithPointer> first;
    private readonly ContactPointsBufferSegment segment;

    public ContactPointsBufferSpan(ContactPointsBufferSegment segment, int segmentStart) {
        first = segment.GetData(segmentStart);
        this.segment = segment;
    }

    public ref ContactPointWithPointer this[int index] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            Span<ContactPointWithPointer> span = first;
            ContactPointsBufferSegment? segment = this.segment;

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
