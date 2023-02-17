using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Jobs2;

public sealed class Entity {

    internal ArchetypeChunk? chunk;
    internal nint index;

    public bool IsDespawned => chunk is null;
    public EntityWorld World => (chunk ?? throw NewObjectDisposedException()).Archetype.World;

    internal Entity(ArchetypeChunk chunk, nint index) {
        this.chunk = chunk;
        this.index = index;
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

    public bool Contains(Type type) {
        ArchetypeChunk? chunk = this.chunk;
        if (chunk is null)
            return false;
        return chunk.Offsets.ContainsKey(type);
    }

    /// <summary>
    /// Tries returns T component of this <see cref="Entity"/>.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="IComponent"/>.</typeparam>
    /// <param name="value">
    /// Copy or reference to component attached to this <see cref="Entity"/> or when result is
    /// <see langword="false"/> default value.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when this <see cref="Entity"/> contains T component; otherwise <see langword="false"/>.
    /// </returns>
    public bool TryGet<T>([NotNullWhen(true)] out T? value) where T : IComponent {
        do {
            ArchetypeChunk? chunk = this.chunk;
            nint index = this.index;

            if (chunk is null || !chunk.Offsets.TryGetValue(typeof(T), out nint offset)) {
                value = default;
                return false;
            }

            unsafe {
                fixed (byte* ptr = chunk.StorageData) {
                    byte* pi = ptr + index;
                    value = Unsafe.AsRef<T>(ptr + index + offset);

                    if (Unsafe.AsRef<EntityInternalComponent>(pi).Entity is null)
                        continue;
                }
            }

            if (chunk == this.chunk || index == this.index)
                return true;
        } while (true);
    }

    private ObjectDisposedException NewObjectDisposedException() {
        return new ObjectDisposedException(nameof(Entity));
    }

}
