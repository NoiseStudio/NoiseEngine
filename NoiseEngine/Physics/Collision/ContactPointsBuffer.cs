using NoiseEngine.Jobs;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NoiseEngine.Physics.Collision;

internal class ContactPointsBuffer {

    private readonly ConcurrentDictionary<Entity, int> pointers = new ConcurrentDictionary<Entity, int>();
    private readonly ConcurrentDictionary<(Entity, Entity, int), int> earlierPoints = 
        new ConcurrentDictionary<(Entity, Entity, int), int>();

    private ContactPointsBufferSegment<ContactData> points = new ContactPointsBufferSegment<ContactData>(1024);
    private int nextPoint = 1;
    private ushort frameId;

    public void NextFrame() {
        frameId++;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public ref ContactData GetData(Entity current, Entity other, int convexHullId) {
        int pointer = earlierPoints.GetOrAdd((current, other, convexHullId), CreateEarlierPointer);
        AppendPointer(current, pointer);

        ref ContactWithPointer<ContactData> data = ref points[pointer];
        data.Pointer = 0;

        if (frameId - data.Element.Manifold.FrameId >= 2)
            data.Element.Manifold.Clear();
        data.Element.Manifold.FrameId = frameId;

        return ref data.Element;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool TryIterateThrough(Entity entity, out ContactPointsBufferIterator iterator) {
        if (pointers.TryGetValue(entity, out int pointer)) {
            iterator = new ContactPointsBufferIterator(points, pointer);
            return true;
        }

        iterator = default;
        return false;
    }

    public void Clear() {
        if (points.Next is not null)
            points = new ContactPointsBufferSegment<ContactData>(Math.Max(points.Size, nextPoint));

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

    private int CreateEarlierPointer((Entity, Entity, int) key) {
        return Interlocked.Increment(ref nextPoint) - 1;
    }

}
