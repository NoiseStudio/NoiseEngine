using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NoiseEngine.Jobs2;

internal class ArchetypeChunk {

    private readonly nint columnSize;
    private readonly Array storage;
    private int count = -1;

    public Archetype Archetype { get; }

    internal byte[] StorageData { get; }
    internal Dictionary<Type, nint> Offsets { get; }

    public ArchetypeChunk(Archetype archetype, Type columnType, nint columnSize) {
        Archetype = archetype;
        Offsets = archetype.Offsets;
        this.columnSize = columnSize;

        nint length = 16000 / columnSize;
        storage = Array.CreateInstance(columnType, length == 0 ? 1 : length);

        StorageData = Unsafe.As<byte[]>(storage);
    }

    public bool TryTakeRecord(out nint index) {
        if (count < storage.Length) {
            int i = Interlocked.Increment(ref count);
            if (i < storage.Length) {
                index = i * columnSize;
                return true;
            }
        }

        index = default;
        return false;
    }

    public void CopyToExtensive(nint sourceIndex, ArchetypeChunk destinationChunk, nint destinationIndex) {

    }

    public void EnterWriteLock(nint index) {
    }

    public void ExitWriteLock(nint index) {
    }

    public void EnterReadLock(nint index) {
    }

    public void ExitReadLock(nint index) {
    }

}
