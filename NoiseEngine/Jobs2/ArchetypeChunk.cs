using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Reflection;
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

    public bool TryReadAnyRecord(
        IEnumerable<Type> componentsToRead, [NotNullWhen(true)] out Dictionary<Type, object>? components
    ) {
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
                    if (Unsafe.AsRef<EntityInternalComponent>((void*)i).Entity is null)
                        continue;

                    components = new Dictionary<Type, object>();
                    foreach (Type type in componentsToRead) {
                        nint size = Archetype.ComponentTypes.First(x => x.type == type).size;
                        object obj = Activator.CreateInstance(type, true) ?? throw new UnreachableException();

                        fixed (byte* vp = &Unsafe.As<object, byte>(ref obj)) {
                            Buffer.MemoryCopy(
                                (void*)(i + Offsets[type]),
                                (void*)(Unsafe.Read<IntPtr>(vp) + sizeof(nint)), size, size
                            );
                        }

                        components.Add(type, obj);
                    }

                    if (Unsafe.AsRef<EntityInternalComponent>((void*)i).Entity is not null)
                        return true;
                }
            }
        }

        components = null;
        return false;
    }

}
