using NoiseEngine.Collections.Concurrent;
using NoiseEngine.Threading;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NoiseEngine.Jobs;

internal class Archetype {

    internal readonly ConcurrentList<ArchetypeChunk> chunks = new ConcurrentList<ArchetypeChunk>();

    private readonly Type columnType;
    private readonly ConcurrentQueue<(ArchetypeChunk, nint)> releasedRecords =
        new ConcurrentQueue<(ArchetypeChunk, nint)>();
    private readonly ConcurrentQueue<AffectiveSystem> affectiveSystems =
        new ConcurrentQueue<AffectiveSystem>();

    private AtomicBool isInitialized;
    private uint queuedAffectiveSystems;
    private ArchetypeChunk lastChunk;

    public EntityWorld World { get; }
    public int HashCode { get; }

    internal nint RecordSize { get; }
    internal (Type type, int size, int affectiveHashCode)[] ComponentTypes { get; }
    internal Dictionary<Type, nint> Offsets { get; } = new Dictionary<Type, nint>();
    internal Dictionary<Type, int> HashCodes { get; } = new Dictionary<Type, int>();
    internal ConcurrentDictionary<Type, ChangedObserverContext[]> ChangedObserversLookup { get; } =
        new ConcurrentDictionary<Type, ChangedObserverContext[]>();

    public Archetype(
        EntityWorld world, int hashCode, IReadOnlyList<(Type type, int affectiveHashCode)> componentTypes
    ) {
        componentTypes = componentTypes.OrderBy(x => x.type.FullName).ToArray();

        World = world;
        HashCode = hashCode;
        ComponentTypes = new (Type, int, int)[componentTypes.Count];

        Type? columnType = null;
        for (int i = componentTypes.Count - 1; i >= 0; i--) {
            if (columnType is null)
                columnType = componentTypes[i].type;
            else
                columnType = typeof(ArchetypeColumn<,>).MakeGenericType(componentTypes[i].type, columnType);
        }

        if (columnType is null) {
            columnType = typeof(EntityInternalComponent);
            Offsets.Add(typeof(EntityInternalComponent), 0);
            RecordSize = Unsafe.SizeOf<EntityInternalComponent>();
        } else {
            columnType = typeof(ArchetypeColumn<,>).MakeGenericType(typeof(EntityInternalComponent), columnType);
            ArchetypeColumn<nint, nint> offsets = (ArchetypeColumn<nint, nint>)columnType.GetMethod(
                nameof(ArchetypeColumn<nint, nint>.GetOffsets)
            )!.Invoke(null, null)!;

            Offsets.Add(typeof(EntityInternalComponent), offsets.Element1);
            nint offset = offsets.Element2;

            Type columnTypeFragment = columnType;
            (Type type, int affectiveHashCode) componentType;

            for (int i = 0; i < componentTypes.Count - 1; i++) {
                columnTypeFragment = columnTypeFragment.GetField(
                    nameof(ArchetypeColumn<nint, nint>.Element2)
                )!.FieldType;
                offsets = (ArchetypeColumn<nint, nint>)columnTypeFragment.GetMethod(
                    nameof(ArchetypeColumn<nint, nint>.GetOffsets)
                )!.Invoke(null, null)!;

                componentType = componentTypes[i];
                Offsets.Add(componentType.type, offset + offsets.Element1);
                ComponentTypes[i] =
                    (componentType.type, (int)(offsets.Element2 - offsets.Element1), componentType.affectiveHashCode);

                offset += offsets.Element2;
            }

            RecordSize = (nint)columnType.GetMethod(nameof(ArchetypeColumn<nint, nint>.GetSize))!.Invoke(null, null)!;

            componentType = componentTypes[componentTypes.Count - 1];
            Offsets.Add(componentType.type, offset);
            ComponentTypes[componentTypes.Count - 1] =
                (componentType.type, (int)(RecordSize - offset), componentType.affectiveHashCode);
        }

        this.columnType = columnType;

        foreach ((Type type, int affectiveHashCode) in componentTypes)
            HashCodes.Add(type, type.GetHashCode() + affectiveHashCode * 16777619);

        // Enqueue affective systems.
        foreach (AffectiveSystem affectiveSystem in world.AffectiveSystems)
            RegisterAffectiveSystem(affectiveSystem);

        lastChunk = new ArchetypeChunk(this, columnType);
        chunks.Add(lastChunk);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    internal static int GetComponentHashCode<T>(T component) where T : IComponent {
        if (component is IAffectiveComponent<T> affectiveComponent)
            return unchecked(typeof(T).GetHashCode() + (affectiveComponent.GetAffectiveHashCode() * 16777619));
        return typeof(T).GetHashCode();
    }

    public void Initialize() {
        if (isInitialized.Exchange(true))
            return;

        foreach (EntitySystem system in World.Systems)
            system.RegisterArchetype(this);

        foreach (WeakReference<EntityQuery> query in World.Queries) {
            if (query.TryGetTarget(out EntityQuery? q))
                q.RegisterArchetype(this);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void InitializeRecord() {
        if (queuedAffectiveSystems > 0)
            InitializeRecordWorker();
    }

    public (ArchetypeChunk chunk, nint index) TakeRecord() {
        if (releasedRecords.TryDequeue(out (ArchetypeChunk, nint) o))
            return o;

        ArchetypeChunk chunk = lastChunk;
        if (chunk.TryTakeRecord(out nint index))
            return (chunk, index);

        lock (chunks) {
            while (true) {
                if (releasedRecords.TryDequeue(out o))
                    return o;

                chunk = lastChunk;
                if (chunk.TryTakeRecord(out index))
                    return (chunk, index);

                lastChunk = new ArchetypeChunk(this, columnType);
                if (!lastChunk.TryTakeRecord(out nint i))
                    continue;

                chunks.Add(lastChunk);
                return (lastChunk, i);
            }
        }
    }

    public void ReleaseRecord(ArchetypeChunk chunk, nint index) {
        Debug.Assert(index % RecordSize == 0);
        releasedRecords.Enqueue((chunk, index));
    }

    public bool TryReadAnyRecord([NotNullWhen(true)] out Dictionary<Type, IComponent>? components) {
        foreach (ArchetypeChunk chunk in chunks) {
            if (chunk.TryReadAnyRecord(out components))
                return true;
        }

        components = null;
        return false;
    }

    public void RegisterAffectiveSystem(AffectiveSystem affectiveSystem) {
        if (affectiveSystem.AffectiveComponents.All(Offsets.ContainsKey)) {
            Interlocked.Increment(ref queuedAffectiveSystems);
            affectiveSystems.Enqueue(affectiveSystem);
        }
    }

    private void InitializeRecordWorker() {
        if (!TryReadAnyRecord(out Dictionary<Type, IComponent>? components))
            return;

        while (affectiveSystems.TryDequeue(out AffectiveSystem? affectiveSystem)) {
            Interlocked.Decrement(ref queuedAffectiveSystems);
            affectiveSystem.RegisterArchetype(this, components);
        }
    }

}
