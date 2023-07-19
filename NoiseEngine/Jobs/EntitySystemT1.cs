using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Jobs;

public abstract class EntitySystem<TThreadStorage> : EntitySystem
    where TThreadStorage : IThreadStorage<TThreadStorage>
{

    protected IReadOnlyCollection<TThreadStorage> ThreadStorages {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        get {
#pragma warning disable CS0618
            object? threadStorages = NoiseEngineInternal_DoNotUse_Storage.ThreadStorages;
#pragma warning restore CS0618
            Debug.Assert(threadStorages is not null, "ThreadStorages is null.");
            return Unsafe.As<ConcurrentStack<TThreadStorage>>(threadStorages);
        }
    }

}
