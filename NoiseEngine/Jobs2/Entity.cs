using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Jobs2;

public sealed class Entity : IDisposable {

    private ArchetypeChunk? chunk;
    private nint index;

    public bool IsDespawned => chunk is null;

    internal Entity(ArchetypeChunk chunk, nint index) {
        this.chunk = chunk;
        this.index = index;
    }

    /// <summary>
    /// Despawns this <see cref="Entity"/>.
    /// </summary>
    public void Despawn() {
        if (!EnterReadLock(out ArchetypeChunk? chunk))
            return;

        nint oldIndex = index;
        this.chunk = null;
        index = -1;

        unsafe {
            fixed (byte* sp = chunk.StorageData)
                new Span<byte>(sp + oldIndex, (int)chunk.Archetype.RecordSize).Clear();
        }

        chunk!.ExitWriteLock(oldIndex);
        chunk.Archetype.ReleaseRecord(chunk, oldIndex);
    }

    /// <summary>
    /// Disposes this <see cref="Entity"/>.  Does the same as <see cref="Despawn"/> method.
    /// </summary>
    public void Dispose() {
        Despawn();
    }

    /// <summary>
    /// Checks if this <see cref="Entity"/> contains T component.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="IComponent"/>.</typeparam>
    /// <returns>
    /// <see langword="true"/> when this <see cref="Entity"/> contains T component; otherwise <see langword="false"/>.
    /// </returns>
    public bool Contains<T>() where T : IComponent {
        ArchetypeChunk? chunk = this.chunk;
        if (chunk is null)
            return false;
        return chunk.Offsets.ContainsKey(typeof(T));
    }

    /// <summary>
    /// Tries returns T component of this <see cref="Entity"/>.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="IComponent"/>.</typeparam>
    /// <param name="value">
    /// Component attached to this <see cref="Entity"/> or when result is <see langword="false"/> default value.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when this <see cref="Entity"/> contains T component; otherwise <see langword="false"/>.
    /// </returns>
    public bool TryGet<T>([NotNullWhen(true)] out T? value) where T : IComponent {
        if (!EnterReadLock(out ArchetypeChunk? chunk)) {
            value = default;
            return false;
        }

        if (!chunk.Offsets.TryGetValue(typeof(T), out nint offset)) {
            chunk.ExitReadLock(index);
            value = default;
            return false;
        }

        offset += index;
        unsafe {
            fixed (byte* ptr = chunk.StorageData)
                value = Unsafe.AsRef<T>(ptr + offset);
        }

        chunk.ExitReadLock(index);
        return true;
    }

    /// <summary>
    /// Tries adds T component to this <see cref="Entity"/>.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="IComponent"/>.</typeparam>
    /// <param name="value">The value of the added component.</param>
    /// <returns>
    /// <see langword="true"/> when T component with <paramref name="value"/> was added to this <see cref="Entity"/>;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public bool TryAdd<T>(T value) where T : IComponent {
        ArchetypeChunk? chunk = this.chunk;
        if (chunk is null || chunk.Offsets.ContainsKey(typeof(T)) || !EnterWriteLock(out chunk))
            return false;

        if (chunk.Offsets.ContainsKey(typeof(T))) {
            chunk.ExitWriteLock(index);
            return false;
        }

        (ArchetypeChunk newChunk, nint newIndex) = chunk.Archetype.GetArchetypeWith<T>().TakeRecord();
        nint oldIndex = index;

        this.chunk = newChunk;
        index = newIndex;

        nint offset = newChunk.Offsets[typeof(T)];
        unsafe {
            fixed (byte* dp = newChunk.StorageData) {
                byte* di = dp + newIndex;

                // Set new component.
                Unsafe.AsRef<T>(di + offset) = value;

                fixed (byte* sp = chunk.StorageData) {
                    byte* si = sp + oldIndex;

                    // Copy old components.
                    foreach ((Type type, int size) in chunk.Archetype.ComponentTypes)
                        Buffer.MemoryCopy(si + chunk.Offsets[type], di + newChunk.Offsets[type], size, size);

                    // Copy internal component.
                    int iSize = Unsafe.SizeOf<EntityInternalComponent>();
                    Buffer.MemoryCopy(si, di, iSize, iSize);

                    // Clear old data.
                    new Span<byte>(si, (int)chunk.Archetype.RecordSize).Clear();
                }
            }
        }

        chunk.ExitWriteLock(oldIndex);
        chunk.Archetype.ReleaseRecord(chunk, oldIndex);

        return true;
    }

    /// <summary>
    /// Tries removes T component from this <see cref="Entity"/>.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="IComponent"/>.</typeparam>
    /// <returns>
    /// <see langword="true"/> when T component was removed from this <see cref="Entity"/> or this <see cref="Entity"/>
    /// did not have T component; otherwise <see langword="false"/>.
    /// </returns>
    public bool TryRemove<T>() where T : IComponent {
        ArchetypeChunk? chunk = this.chunk;
        if (chunk is null || !chunk.Offsets.ContainsKey(typeof(T)) || !EnterWriteLock(out chunk))
            return false;

        if (!chunk.Offsets.ContainsKey(typeof(T))) {
            chunk.ExitWriteLock(index);
            return false;
        }

        (ArchetypeChunk newChunk, nint newIndex) = chunk.Archetype.GetArchetypeWithout(typeof(T)).TakeRecord();
        nint oldIndex = index;

        this.chunk = newChunk;
        index = newIndex;

        unsafe {
            fixed (byte* dp = newChunk.StorageData) {
                byte* di = dp + newIndex;
                fixed (byte* sp = chunk.StorageData) {
                    byte* si = sp + oldIndex;

                    // Copy old components.
                    foreach ((Type type, int size) in chunk.Archetype.ComponentTypes) {
                        if (type != typeof(T))
                            Buffer.MemoryCopy(si + chunk.Offsets[type], di + newChunk.Offsets[type], size, size);
                    }

                    // Copy internal component.
                    int iSize = Unsafe.SizeOf<EntityInternalComponent>();
                    Buffer.MemoryCopy(si, di, iSize, iSize);

                    // Clear old data.
                    new Span<byte>(si, (int)chunk.Archetype.RecordSize).Clear();
                }
            }
        }

        chunk.ExitWriteLock(oldIndex);
        chunk.Archetype.ReleaseRecord(chunk, oldIndex);

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool EnterWriteLock([NotNullWhen(true)] out ArchetypeChunk? chunk) {
        do {
            chunk = this.chunk;
            if (chunk is null)
                return false;

            chunk.EnterWriteLock(index);
            if (chunk == this.chunk)
                return true;
            chunk.ExitWriteLock(index);
        } while (true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool EnterReadLock([NotNullWhen(true)] out ArchetypeChunk? chunk) {
        do {
            chunk = this.chunk;
            if (chunk is null)
                return false;

            chunk.EnterReadLock(index);
            if (chunk == this.chunk)
                return true;
            chunk.ExitReadLock(index);
        } while (true);
    }

    private ObjectDisposedException NewObjectDisposedException() {
        return new ObjectDisposedException(nameof(Entity));
    }

}
