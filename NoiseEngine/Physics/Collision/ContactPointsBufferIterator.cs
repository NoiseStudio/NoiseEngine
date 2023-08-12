using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics.Collision;

internal ref struct ContactPointsBufferIterator {

    public ref ContactPoint Current;

    private readonly ContactPointsBufferSegment points;
    private int pointer;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ContactPointsBufferIterator(ContactPointsBufferSegment points, int pointer) {
        this.points = points;

        ref ContactPointWithPointer current = ref points[pointer];
        Current = ref current.Point;
        this.pointer = current.Pointer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool MoveNext() {
        if (pointer == 0)
            return false;

        ref ContactPointWithPointer current = ref points[pointer];
        Current = ref current.Point;
        pointer = current.Pointer;
        return true;
    }

}
