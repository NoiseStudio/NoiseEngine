using NoiseEngine.Jobs;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NoiseEngine.Physics.Collision;

internal class ContactPointsBuffer {

    private readonly ConcurrentDictionary<Entity, int> pointers = new ConcurrentDictionary<Entity, int>();

    private ContactPointsBufferSegment points = new ContactPointsBufferSegment(2560);
    private int nextPoint = 1;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public void Add(Entity entity, ContactPoint point) {
        int pointer = Interlocked.Increment(ref nextPoint) - 1;
        points[pointer] = new ContactPointWithPointer(point, 0);
        AppendPointer(entity, pointer);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public void Add(Entity entity, ReadOnlySpan<ContactPoint> points) {
        int pointer = Interlocked.Add(ref nextPoint, points.Length) - points.Length;
        ContactPointsBufferSpan span = this.points.AsSpan(pointer);

        int max = points.Length - 1;
        for (int i = 0; i < max;)
            span[i] = new ContactPointWithPointer(points[i++], pointer + i);
        span[max] = new ContactPointWithPointer(points[max], 0);

        AppendPointer(entity, pointer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public ContactPointsBufferIterator IterateThrough(Entity entity) {
        if (pointers.TryGetValue(entity, out int pointer))
            return new ContactPointsBufferIterator(points, pointer);
        return default;
    }

    public void Clear() {
        if (points.Next is not null)
            points = new ContactPointsBufferSegment(Math.Max(points.Size, nextPoint));

        pointers.Clear();
        nextPoint = 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private void AppendPointer(Entity entity, int pointer) {
        int startPointer = pointers.GetOrAdd(entity, pointer);
        if (startPointer == pointer)
            return;

        while ((startPointer = Interlocked.CompareExchange(ref points[startPointer].Pointer, pointer, 0)) != 0)
            continue;

        Debug.Assert(startPointer == 0);
    }

}
