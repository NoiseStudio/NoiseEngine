using NoiseEngine.Collections.Concurrent;
using NoiseEngine.Threading;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NoiseEngine.Jobs2;

public abstract class EntityQuery : IDisposable {

    internal readonly ConcurrentList<Archetype> archetypes = new ConcurrentList<Archetype>();

    private IEntityFilter? filter;
    private AtomicBool isDisposed;

    public EntityWorld World { get; }
    public bool IsDisposed => isDisposed;

    internal abstract IEnumerable<Type> UsedComponentsInternal { get; }

    public IEntityFilter? Filter {
        get => filter;
        set {
            lock (archetypes) {
                filter = value;
                archetypes.Clear();

                AssertIsNotDisposed();
                foreach (Archetype archetype in World.Archetypes)
                    RegisterArchetype(archetype);
            }
        }
    }

    internal WeakReference<EntityQuery> Weak { get; }

    private protected EntityQuery(EntityWorld world) {
        World = world;
        Weak = new WeakReference<EntityQuery>(this);
    }

    ~EntityQuery() {
        ReleaseResources();
    }

    /// <summary>
    /// Disposes this <see cref="EntityQuery"/>.
    /// </summary>
    public void Dispose() {
        if (isDisposed.Exchange(true))
            return;

        ReleaseResources();
        GC.SuppressFinalize(this);
    }

    internal void RegisterArchetype(Archetype archetype) {
        foreach (Type type in UsedComponentsInternal) {
            if (!archetype.Offsets.ContainsKey(type))
                return;
        }

        if (Filter?.CompareComponents(archetype.ComponentTypes.Select(x => new ComponentType(
            x.type, x.affectiveHashCode
        ))) == false) {
            return;
        }

        lock (archetypes) {
            if (!archetypes.Contains(archetype))
                archetypes.Add(archetype);
        }
    }

    private void ReleaseResources() {
        World.RemoveQuery(this);
        archetypes.Clear();
        filter = null;
    }

    private void AssertIsNotDisposed() {
        if (isDisposed)
            throw new ObjectDisposedException(ToString(), "Entity query is disposed.");
    }

}
