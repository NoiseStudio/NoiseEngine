﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace NoiseEngine.Jobs;

internal class ArchetypeChunk {

    private readonly EntityLocker locker = new EntityLocker();
    private readonly Array storage;
    private readonly int sizeInBytes;
    private int count = -1;

    public Archetype Archetype { get; }
    public int ArchetypeHashCode { get; }
    public int Capacity { get; }
    public int Count => Math.Min(count, CapacityM);

    internal nint RecordSize { get; }
    internal byte[] StorageData { get; }
    internal Dictionary<Type, nint> Offsets { get; }
    internal Dictionary<Type, int> HashCodes { get; }
    internal Dictionary<Type, (nint offset, int size)> ExtendedInformation { get; }
    internal ConcurrentDictionary<Type, ChangedObserverContext[]> ChangedObserversLookup { get; }

    internal Span<byte> StorageDataSpan {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => MemoryMarshal.CreateSpan(ref StorageData[0], sizeInBytes);
    }

    private int CapacityM { get; }

    public ArchetypeChunk(Archetype archetype, Type columnType) {
        Archetype = archetype;
        ArchetypeHashCode = archetype.HashCode;
        Offsets = archetype.Offsets;
        HashCodes = archetype.HashCodes;
        ExtendedInformation = archetype.ExtendedInformation;
        ChangedObserversLookup = archetype.ChangedObserversLookup;
        RecordSize = archetype.RecordSize;

        int capacity = (int)(16000 / RecordSize);
        Capacity = capacity == 0 ? 1 : capacity;
        CapacityM = Capacity - 1;
        storage = Array.CreateInstance(columnType, Capacity);

        StorageData = Unsafe.As<byte[]>(storage);
        sizeInBytes = (int)(Capacity * RecordSize);
    }

    public bool TryTakeRecord(out nint index) {
        if (count < CapacityM) {
            int i = Interlocked.Increment(ref count);
            if (i < CapacityM) {
                index = i * RecordSize;
                return true;
            }

            Interlocked.Decrement(ref count);
        }

        index = default;
        return false;
    }

    public EntityLocker GetLocker() {
        return locker;
    }

    public bool TryReadAnyRecord([NotNullWhen(true)] out Dictionary<Type, IComponent>? components) {
        if (count < 0) {
            components = null;
            return false;
        }

        unsafe {
            fixed (byte* ptr = StorageData) {
                nint end = (count + 1) * RecordSize + (nint)ptr;
                int j = -1;
                for (nint i = (nint)ptr; i < end; i += RecordSize) {
                    j++;
                    Entity? entity = Unsafe.AsRef<EntityInternalComponent>((void*)i).Entity;
                    if (entity is null)
                        continue;

                    components = new Dictionary<Type, IComponent>();
                    foreach ((Type type, int size, _) in Archetype.ComponentTypes)
                        components.Add(type, ReadComponentBoxed(type, size, i + Offsets[type]));

                    Entity? finalEntity = Unsafe.AsRef<EntityInternalComponent>((void*)i).Entity;
                    if (finalEntity == entity)
                        return true;
                    if (finalEntity is not null)
                        i -= RecordSize;
                }
            }
        }

        components = null;
        return false;
    }

    public unsafe IComponent ReadComponentBoxed(Type type, int size, nint componentPointer) {
        IComponent obj;
        if (size == sizeof(nint) && !type.IsValueType) {
            obj = null!;
            fixed (byte* vp = &Unsafe.As<IComponent, byte>(ref obj))
                Buffer.MemoryCopy((void*)componentPointer, vp, size, size);
            return obj;
        }

        obj = Unsafe.As<IComponent>(Activator.CreateInstance(type, true) ?? throw new UnreachableException());
        fixed (byte* vp = &Unsafe.As<IComponent, byte>(ref obj)) {
            Buffer.MemoryCopy(
                (void*)componentPointer,
                (void*)(Unsafe.Read<IntPtr>(vp) + sizeof(nint)), size, size
            );
        }
        return obj;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ChangedObserverContext[] GetChangedObservers(Type changedComponentType) {
        if (ChangedObserversLookup.TryGetValue(changedComponentType, out ChangedObserverContext[]? observers))
            return observers;
        return CreateChangedObservers(changedComponentType);
    }

    private ChangedObserverContext[] CreateChangedObservers(Type changedComponentType) {
        lock (Archetype.World.changedObservers) {
            ChangedObserverContext[] observers = CreateObservers(EntityObserverType.Changed, changedComponentType);
            ChangedObserversLookup.TryAdd(changedComponentType, observers);
        }
        return ChangedObserversLookup[changedComponentType];
    }

    private ChangedObserverContext[] CreateObservers(EntityObserverType type, Type componentType) {
        List<ChangedObserverContext> result = new List<ChangedObserverContext>();
        foreach (ChangedObserverContext context in Archetype.World.changedObservers) {
            MethodInfo method = context.Observer.Method;
            if (
                method.GetCustomAttributes<WithoutAttribute>().SelectMany(x => x.Components)
                    .Any(Offsets.ContainsKey)
            ) {
                continue;
            }
            if (
                method.GetCustomAttributes<WithAttribute>().SelectMany(x => x.Components)
                    .Any(x => !Offsets.ContainsKey(x))
            ) {
                continue;
            }

            ParameterInfo[] parameters = context.Observer.Method.GetParameters();
            int skip = parameters[1].ParameterType == typeof(SystemCommands) ? 2 : 1;

            if (parameters[skip].ParameterType != type switch {
                EntityObserverType.Changed => typeof(Changed<>).MakeGenericType(componentType),
                _ => throw new NotImplementedException(),
            }) {
                continue;
            }

            if (parameters.Skip(skip + 1).Select(x => x.ParameterType).Any(x => !Offsets.ContainsKey(x)))
                continue;

            result.Add(context);
        }

        return result.ToArray();
    }

}
