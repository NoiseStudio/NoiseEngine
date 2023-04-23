using NoiseEngine.Collections;
using NoiseEngine.Collections.Concurrent;
using NoiseEngine.Jobs2.Commands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace NoiseEngine.Jobs2;

public class EntityWorld : IDisposable {

    private readonly ConcurrentDictionary<EquatableReadOnlyList<(Type, int)>, Archetype> archetypes =
        new ConcurrentDictionary<EquatableReadOnlyList<(Type, int)>, Archetype>();
    private readonly ConcurrentList<EntitySystem> systems = new ConcurrentList<EntitySystem>();
    private readonly ConcurrentDictionary<Type, MethodInfo> affectiveSystems =
        new ConcurrentDictionary<Type, MethodInfo>();
    private readonly object despawnQueueLocker = new object();

    private ConcurrentQueue<Entity>? despawnQueue;
    private AutoResetEvent? despawnQueueResetEvent;
    private bool despawnQueueThreadWork;

    public EntitySchedule? DefaultSchedule { get; set; }

    internal IEnumerable<EntitySystem> Systems => systems;

    public EntityWorld() {
        DefaultSchedule = Application.EntitySchedule2;
    }

    public void Dispose() {
        //throw new NotImplementedException();
    }

    public void AddSystem(EntitySystem system, double? cycleTime = null) {
        system.InternalInitialize(this);
        if (cycleTime.HasValue)
            system.CycleTime = cycleTime.Value;

        systems.Add(system);
        RegisterArchetypesToSystem(system);
    }

    public void AddAffectiveSystem(AffectiveSystem system) {

    }

    /// <summary>
    /// Executes <paramref name="commands"/> in current thread.
    /// </summary>
    /// <param name="commands"><see cref="SystemCommands"/> to execute.</param>
    public void ExecuteCommands(SystemCommands commands) {
        if (ApplicationJitConsts.IsDebugMode) {
            EntitySchedule.AssertNotScheduleLockThread(
                "Use built in `SystemCommands` parameter in `OnUpdateEntity` instead."
            );
        }

        SystemCommandsInner inner = commands.Inner;
        FastList<SystemCommand> c = inner.Commands;
        inner.Consume();

        if (c.Count == 0)
            return;

        new SystemCommandsExecutor(c).Invoke();
    }

    /// <summary>
    /// Asynchronous executes <paramref name="commands"/> and returns <see cref="Task"/> that will be completed when all
    /// commands will be executed.
    /// </summary>
    /// <param name="commands"><see cref="SystemCommands"/> to execute.</param>
    /// <returns><see cref="Task"/> that will be completed when all commands will be executed.</returns>
    public Task ExecuteCommandsAsync(SystemCommands commands) {
        SystemCommandsInner inner = commands.Inner;
        FastList<SystemCommand> c = inner.Commands;
        inner.Consume();

        if (c.Count == 0)
            return Task.CompletedTask;
        return Task.Run(() => new SystemCommandsExecutor(c).Invoke());
    }

    /// <summary>
    /// Spawns new <see cref="Entity"/> without components.
    /// </summary>
    /// <returns>New <see cref="Entity"/>.</returns>
    public Entity Spawn() {
        Archetype archetype = GetArchetype(Array.Empty<(Type, int)>(), () => Array.Empty<(Type, int, int)>());
        (ArchetypeChunk chunk, nint index) = archetype.TakeRecord();
        Entity entity = new Entity(chunk, index);

        unsafe {
            fixed (byte* ptr = chunk.StorageData) {
                byte* pointer = ptr + index;

                Unsafe.AsRef<EntityInternalComponent>(pointer) = new EntityInternalComponent(entity);
            }
        }

        return entity;
    }

    /// <summary>
    /// Spawns new <see cref="Entity"/> with specified components.
    /// </summary>
    /// <typeparam name="T1">Type of <see cref="IComponent"/>.</typeparam>
    /// <param name="component1">Value of T1 component.</param>
    /// <returns>New <see cref="Entity"/>.</returns>
    public Entity Spawn<T1>(T1 component1) where T1 : IComponent {
        Archetype archetype = GetArchetype(new (Type, int)[] {
            (typeof(T1), IAffectiveComponent.GetAffectiveHashCode(component1))
        }, () => new (Type type, int size, int affectiveHashCode)[] {
            (typeof(T1), Unsafe.SizeOf<T1>(), IAffectiveComponent.GetAffectiveHashCode(component1))
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
    internal Archetype GetArchetype(
        IEnumerable<(Type type, int affectiveHashCode)> componentTypes,
        Func<(Type type, int size, int affectiveHashCode)[]> valueFactory
    ) {
        Archetype archetype = archetypes.GetOrAdd(new EquatableReadOnlyList<(Type, int)>(
            componentTypes.OrderBy(x => x.type.GetHashCode()
        ).ToArray()), _ => new Archetype(this, valueFactory()));
        archetype.Initialize();

        return archetype;
    }

    internal void EnqueueToDespawnQueue(Entity entity) {
        if (despawnQueue is null) {
            lock (despawnQueueLocker) {
                if (despawnQueue is null) {
                    despawnQueueResetEvent = new AutoResetEvent(false);
                    despawnQueue = new ConcurrentQueue<Entity>();
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

    internal void RegisterArchetypesToSystem(EntitySystem system) {
        foreach (Archetype archetype in archetypes.Values)
            system.RegisterArchetype(archetype);
    }

    internal void RemoveSystem(EntitySystem system) {
        systems.Remove(system);
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

        resetEvent.Dispose();
    }

}
