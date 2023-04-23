using System;
using System.Threading;

namespace NoiseEngine.Jobs2;

public abstract class AffectiveSystem {

    private EntityWorld? world;

    public EntityWorld World => world ?? throw new InvalidOperationException(
        "This affective system is not initialized."
    );

    private protected abstract Type[] AffectiveComponents { get; }

    internal void InternalInitialize(EntityWorld world) {
        if (Interlocked.CompareExchange(ref this.world, world, null) is not null)
            throw new InvalidOperationException("Affective system is already initialized.");
    }

}
