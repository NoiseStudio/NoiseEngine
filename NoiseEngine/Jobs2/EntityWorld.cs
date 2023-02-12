using NoiseEngine.Collections;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Jobs2;

public partial class EntityWorld {

    private readonly ConcurrentDictionary<EquatableReadOnlyList<Type>, Archetype> archetypes =
        new ConcurrentDictionary<EquatableReadOnlyList<Type>, Archetype>();

    /// <summary>
    /// Spawns new <see cref="Entity"/> with specified components.
    /// </summary>
    /// <typeparam name="T1">Type of <see cref="IComponent"/>.</typeparam>
    /// <param name="component1">Value of T1 component.</param>
    /// <returns>New <see cref="Entity"/>.</returns>
    public Entity Spawn<T1>(T1 component1) where T1 : IComponent {
        Archetype archetype = GetArchetype(new Type[] { typeof(T1) }, () => new (Type type, int size)[] {
            (typeof(T1), Unsafe.SizeOf<T1>())
        });

        (ArchetypeChunk chunk, nint index) = archetype.TakeRecord();
        Entity entity = new Entity(chunk, index);

        unsafe {
            fixed (byte* ptr = chunk.StorageData) {
                byte* pointer = ptr + index;

                Unsafe.AsRef<T1>(pointer + archetype.Offsets[typeof(T1)]) = component1;

                Unsafe.AsRef<EntityInternalComponent>(pointer) = new EntityInternalComponent(entity);
            }
        }

        return entity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Archetype GetArchetype(IEnumerable<Type> componentTypes, Func<(Type type, int size)[]> valueFactory) {
        return archetypes.GetOrAdd(
            new EquatableReadOnlyList<Type>(
                componentTypes.OrderBy(x => x.GetHashCode()).ToArray()
            ), _ => new Archetype(this, valueFactory())
        );
    }

}
