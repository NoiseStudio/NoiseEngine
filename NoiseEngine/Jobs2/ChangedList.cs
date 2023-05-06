using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NoiseEngine.Jobs2;

internal sealed class ChangedList {

    private static readonly ConcurrentDictionary<Type, ConcurrentStack<ChangedList>> pool
        = new ConcurrentDictionary<Type, ConcurrentStack<ChangedList>>();

    private readonly Type type;
    internal readonly int size;

    internal Array buffer;
    internal int count;

    internal ChangedList(Array buffer, Type type, int size) {
        this.buffer = buffer;
        this.type = type;
        this.size = size;
    }

    public static ChangedList Rent<T>() where T : IComponent {
        if (
            pool.TryGetValue(typeof(T), out ConcurrentStack<ChangedList>? stack) &&
            stack.TryPop(out ChangedList? obj)
        ) {
            return obj;
        }

        return new ChangedList(
            Array.CreateInstance(typeof(ArchetypeColumn<nint, T>), 64),
            typeof(T),
            Unsafe.SizeOf<ArchetypeColumn<nint, T>>()
        );
    }

    public void Return() {
        if (!pool.TryGetValue(type, out ConcurrentStack<ChangedList>? stack)) {
            pool.TryAdd(type, new ConcurrentStack<ChangedList>());
            if (!pool.TryGetValue(type, out stack))
                return;
        }

        MemoryMarshal.CreateSpan(ref Unsafe.As<byte[]>(buffer)[0], size * count).Clear();
        count = 0;
        stack.Push(this);
    }

}
