using NoiseEngine.Collections;
using NoiseEngine.Jobs2.Commands;
using NoiseEngine.Jobs2.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace NoiseEngine.Jobs2;

public class EntityWorld {

    private readonly ConcurrentDictionary<EquatableReadOnlyList<Type>, Archetype> archetypes =
        new ConcurrentDictionary<EquatableReadOnlyList<Type>, Archetype>();
    private readonly List<EntitySystem> systems = new List<EntitySystem>();
    private readonly ConcurrentDictionary<Type, MethodInfo> affectiveSystems =
        new ConcurrentDictionary<Type, MethodInfo>();
    private readonly object despawnQueueLocker = new object();

    private ConcurrentQueue<Entity>? despawnQueue;
    private AutoResetEvent? despawnQueueResetEvent;
    private bool despawnQueueThreadWork;

    public void AddSystem<T>(T system, double? cycleTime) where T : EntitySystem,
#pragma warning disable CS0618
        NoiseEngineInternal_DoNotUse.INormalEntitySystem
#pragma warning restore CS0618
    {
        if (Application.IsDebugMode)
            AssertSystemInterfaces<T>();

        system.InternalInitialize(this);
        if (cycleTime.HasValue)
            system.CycleTime = cycleTime.Value;

        lock (systems)
            systems.Add(system);
    }

    public void AddAffectiveSystem<T>() where T : EntitySystem,
#pragma warning disable CS0618
        NoiseEngineInternal_DoNotUse.IAffectiveEntitySystem
#pragma warning restore CS0618
    {
        if (Application.IsDebugMode)
            AssertSystemInterfaces<T>();

        affectiveSystems.GetOrAdd(typeof(T), type => {
            return type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(x => x.Name == "Construct")
                .Single(x => x.GetParameters().Select(x => x.ParameterType).SequenceEqual(T.AffectiveComponents));
        });
    }

    public void ExecuteCommands(SystemCommands commands) {
        new SystemCommandsExecutor(commands.Commands).Invoke();
    }

    public Task ExecuteCommandsAsync(SystemCommands commands) {
        FastList<SystemCommand> c = commands.Commands;
        return Task.Run(() => new SystemCommandsExecutor(c).Invoke());
    }

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

    internal void EnqueueToDespawnQueue(Entity entity) {
        if (despawnQueue is null) {
            lock (despawnQueueLocker) {
                if (despawnQueue is null) {
                    despawnQueue = new ConcurrentQueue<Entity>();
                    despawnQueueResetEvent = new AutoResetEvent(false);
                    despawnQueueThreadWork = true;

                    new Thread(DespawnQueueThreadWorker) {
                        Name = $"{this} {nameof(DespawnQueueThreadWorker)}",
                        IsBackground = true,
                    }.Start();
                }
            }
        }

        despawnQueue.Enqueue(entity);
        despawnQueueResetEvent!.Set();
    }

    private void AssertSystemInterfaces<T>() where T : EntitySystem {
#pragma warning disable CS0618
        if (typeof(T).GetInterfaces().Count(x =>
            x == typeof(NoiseEngineInternal_DoNotUse.INormalEntitySystem) ||
            x == typeof(NoiseEngineInternal_DoNotUse.IAffectiveEntitySystem)
        ) > 1) {
            throw new InvalidOperationException($"{nameof(EntitySystem)} cannot implements {
                typeof(NoiseEngineInternal_DoNotUse.INormalEntitySystem).FullName
            } and {typeof(NoiseEngineInternal_DoNotUse.IAffectiveEntitySystem).FullName} simultaneously.");
        }
#pragma warning restore CS0618
    }

    private void DespawnQueueThreadWorker() {
        AutoResetEvent resetEvent = despawnQueueResetEvent!;
        ConcurrentQueue<Entity> despawnQueue = this.despawnQueue!;

        while (despawnQueueThreadWork) {
            resetEvent.WaitOne();

            while (despawnQueue.TryDequeue(out Entity? entity)) {
                if (!EntityLocker.TryLockEntity(entity, true, out EntityLockerHeld held))
                    continue;

                ArchetypeChunk chunk = entity.chunk!;
                entity.chunk = null;
                nint index = entity.index;
                entity.index = 0;

                unsafe {
                    fixed (byte* dp = chunk.StorageData) {
                        byte* di = dp + index;

                        // Clear old data.
                        new Span<byte>(di, (int)chunk.Archetype.RecordSize).Clear();
                    }
                }

                held.Dispose();
                chunk.Archetype.ReleaseRecord(chunk, index);
            }
        }
    }

}
