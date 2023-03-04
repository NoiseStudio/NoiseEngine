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
    public int Count => count;

    internal nint RecordSize { get; }
    internal byte[] StorageData { get; }
    internal Dictionary<Type, nint> Offsets { get; }

    public ArchetypeChunk(Archetype archetype, Type columnType, nint recordSize) {
        Archetype = archetype;
        Offsets = archetype.Offsets;
        RecordSize = recordSize;

        nint length = 16000 / recordSize;
        storage = Array.CreateInstance(columnType, length == 0 ? 1 : length);

        StorageData = Unsafe.As<byte[]>(storage);
    }

    public bool TryTakeRecord(out nint index) {
        if (count < storage.Length) {
            int i = Interlocked.Increment(ref count);
            if (i < storage.Length) {
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
