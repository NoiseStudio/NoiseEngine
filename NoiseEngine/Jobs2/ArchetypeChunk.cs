using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NoiseEngine.Jobs2;

internal class ArchetypeChunk {

    private readonly EntityLocker locker = new EntityLocker();
    private readonly Array storage;
    private int count = -1;

    public Archetype Archetype { get; }
    public int Capacity { get; }
    public int Count => Math.Min(count, CapacityM);

    internal nint RecordSize { get; }
    internal byte[] StorageData { get; }
    internal Dictionary<Type, nint> Offsets { get; }

    private int CapacityM { get; }

    public ArchetypeChunk(Archetype archetype, Type columnType) {
        Archetype = archetype;
        Offsets = archetype.Offsets;
        RecordSize = archetype.RecordSize;

        int capacity = (int)(16000 / RecordSize);
        Capacity = capacity == 0 ? 1 : capacity;
        CapacityM = Capacity - 1;
        storage = Array.CreateInstance(columnType, Capacity);

        StorageData = Unsafe.As<byte[]>(storage);
    }

    public bool TryTakeRecord(out nint index) {
        if (count < CapacityM) {
            int i = Interlocked.Increment(ref count);
            if (i < CapacityM) {
                index = i * RecordSize;
                return true;
            }
        }

        index = default;
        return false;
    }

    public EntityLocker GetLocker(nint index) {
        return locker;
    }

}
