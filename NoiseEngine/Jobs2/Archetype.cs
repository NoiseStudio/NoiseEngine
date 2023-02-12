using NoiseEngine.Collections.Concurrent;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Jobs2;

internal class Archetype {

    private readonly Type columnType;
    private readonly ConcurrentList<ArchetypeChunk> chunks = new ConcurrentList<ArchetypeChunk>();
    private readonly ConcurrentQueue<(ArchetypeChunk, nint)> releasedRecords =
        new ConcurrentQueue<(ArchetypeChunk, nint)>();
    private readonly ConcurrentDictionary<Type, Archetype> withArchetypes =
        new ConcurrentDictionary<Type, Archetype>();
    private readonly ConcurrentDictionary<Type, Archetype> withoutArchetypes =
        new ConcurrentDictionary<Type, Archetype>();

    public EntityWorld World { get; }

    internal nint RecordSize { get; }
    internal (Type type, int size)[] ComponentTypes { get; }
    internal Dictionary<Type, nint> Offsets { get; } = new Dictionary<Type, nint>();

    public Archetype(EntityWorld world, (Type type, int size)[] componentTypes) {
        World = world;
        ComponentTypes = componentTypes;

        Type[] finalComponentTypes = new Type[componentTypes.Length + 1];
        finalComponentTypes[0] = typeof(EntityInternalComponent);
        Offsets.Add(typeof(EntityInternalComponent), 0);
        nint offset = Unsafe.SizeOf<EntityInternalComponent>();

        int i = 1;
        foreach ((Type type, int size) in componentTypes) {
            finalComponentTypes[i++] = type;
            Offsets.Add(type, offset);
            offset += size;
        }

        columnType = ArchetypeColumnCreator.CreateColumnType(finalComponentTypes);
        RecordSize = offset;
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

            ArchetypeChunk c = new ArchetypeChunk(this, columnType, RecordSize);
            if (!c.TryTakeRecord(out nint i))
                throw new InvalidOperationException();

            chunks.Add(c);
            return (c, i);
        }
    }

    public void ReleaseRecord(ArchetypeChunk chunk, nint index) {
        releasedRecords.Enqueue((chunk, index));
    }

    public Archetype GetArchetypeWith<T>() {
        return withArchetypes.GetOrAdd(typeof(T), type => {
            return World.GetArchetype(ComponentTypes.Select(x => x.type).Append(type), () => {
                (Type type, int size)[] newComponentTypes = new (Type type, int size)[ComponentTypes.Length + 1];
                Array.Copy(ComponentTypes, newComponentTypes, ComponentTypes.Length);
                newComponentTypes[^1] = (type, Unsafe.SizeOf<T>());
                return newComponentTypes;
            });
        });
    }

    public Archetype GetArchetypeWithout(Type type) {
        return withoutArchetypes.GetOrAdd(
            type, _ => World.GetArchetype(
                ComponentTypes.Select(x => x.type).Where(x => x != type),
                () => ComponentTypes.Where(x => x.type != type).ToArray()
            )
        );
    }

}
