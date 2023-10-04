using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics.Collision;

internal ref struct ContactPointsBufferIterator {

    public ref ContactData Current;
    public ref ContactPoint CurrentPoint;

    private readonly ContactPointsBufferSegment<ContactData> points;
    private int pointer;
    private int position;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ContactPointsBufferIterator(ContactPointsBufferSegment<ContactData> points, int pointer) {
        this.points = points;

        ref ContactWithPointer<ContactData> current = ref points[pointer];
        Current = ref current.Element;
        this.pointer = current.Pointer;
        SetCurrentPoint();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool MoveNext() {
        if (position >= Current.Manifold.Count) {
            if (pointer == 0)
                return false;

            ref ContactWithPointer<ContactData> current = ref points[pointer];
            Current = ref current.Element;
            pointer = current.Pointer;
            position = 0;
        }

        SetCurrentPoint();
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetCurrentPoint() {
        Debug.Assert(!Unsafe.IsNullRef(ref Current), $"{nameof(Current)} is null.");

        switch (position++) {
            case 0:
                CurrentPoint = ref Current.Manifold.PointA;
                break;
            case 1:
                CurrentPoint = ref Current.Manifold.PointB;
                break;
            case 2:
                CurrentPoint = ref Current.Manifold.PointC;
                break;
            case 3:
                CurrentPoint = ref Current.Manifold.PointD;
                break;
            default:
                throw new IndexOutOfRangeException();
        }
    }

}
