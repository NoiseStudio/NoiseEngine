using NoiseEngine.Collections.Concurrent;
using NoiseEngine.Threading;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Jobs2;

internal class Archetype {

    internal readonly ConcurrentList<ArchetypeChunk> chunks = new ConcurrentList<ArchetypeChunk>();

    private readonly Type columnType;
    private readonly ConcurrentQueue<(ArchetypeChunk, nint)> releasedRecords =
        new ConcurrentQueue<(ArchetypeChunk, nint)>();

    private AtomicBool isInitialized;

    public EntityWorld World { get; }

    internal nint RecordSize { get; }
    internal (Type type, int size, int affectiveHashCode)[] ComponentTypes { get; }
    internal Dictionary<Type, nint> Offsets { get; } = new Dictionary<Type, nint>();

    public Archetype(EntityWorld world, (Type type, int size, int affectiveHashCode)[] componentTypes) {
        World = world;
        ComponentTypes = componentTypes;

        Offsets.Add(typeof(EntityInternalComponent), 0);
        nint offset = Unsafe.SizeOf<EntityInternalComponent>();
        foreach ((Type type, int size, _) in componentTypes) {
            Offsets.Add(type, offset);
            offset += size;
        }

        Type? columnType = null;
        for (int i = componentTypes.Length - 1; i >= 0; i--) {
            if (columnType is null)
                columnType = componentTypes[i].type;
            else
                columnType = typeof(ArchetypeColumn<,>).MakeGenericType(componentTypes[i].type, columnType);
        }

        if (columnType is null)
            columnType = typeof(EntityInternalComponent);
        else
            columnType = typeof(ArchetypeColumn<,>).MakeGenericType(typeof(EntityInternalComponent), columnType);

        this.columnType = columnType;
        RecordSize = offset;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Initialize() {
        if (isInitialized.Exchange(true))
            return;

        foreach (EntitySystem system in World.Systems)
            system.RegisterArchetype(this);
    }

    public (ArchetypeChunk chunk, nint index) TakeRecord() {
        if (releasedRecords.TryDequeue(out (ArchetypeChunk, nint) o))
            return o;

        foreach (ArchetypeChunk chunk in chunks) {
            if (chunk.TryTakeRecord(out nint index))
                return (chunk, index);
        }

        lock (chunks) {
            if (releasedRecords.TryDequeue(out o))
                return o;

            foreach (ArchetypeChunk chunk in chunks) {
                if (chunk.TryTakeRecord(out nint index))
                    return (chunk, index);
            }

            ArchetypeChunk c = new ArchetypeChunk(this, columnType);
            if (!c.TryTakeRecord(out nint i))
                throw new InvalidOperationException();

            chunks.Add(c);
            return (c, i);
        }
    }

    public void ReleaseRecord(ArchetypeChunk chunk, nint index) {
        releasedRecords.Enqueue((chunk, index));
    }

}
