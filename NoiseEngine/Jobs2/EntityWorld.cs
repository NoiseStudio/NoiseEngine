using NoiseEngine.Collections;
using NoiseEngine.Collections.Concurrent;
using NoiseEngine.Jobs2.Commands;
using NoiseEngine.Threading;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks;

namespace NoiseEngine.Jobs2;

public partial class EntityWorld : IDisposable {

    private readonly Dictionary<int, Archetype> archetypes = new Dictionary<int, Archetype>();
    private readonly ConcurrentList<AffectiveSystem> affectiveSystems = new ConcurrentList<AffectiveSystem>();
    private readonly ConcurrentList<EntitySystem> systems = new ConcurrentList<EntitySystem>();
    private readonly object despawnQueueLocker = new object();

    private ConcurrentQueue<Entity>? despawnQueue;
    private AutoResetEvent? despawnQueueResetEvent;
    private bool despawnQueueThreadWork;
    private AtomicBool isDisposed;

    public EntitySchedule? DefaultSchedule { get; set; }

    public bool IsDisposed => isDisposed;
    public IEnumerable<AffectiveSystem> AffectiveSystems => affectiveSystems;
    public IEnumerable<EntitySystem> Systems => systems;

    public EntityWorld() {
        DefaultSchedule = Application.EntitySchedule2;
    }

    /// <summary>
    /// Disposes this <see cref="EntityWorld"/> and it's <see cref="Entity"/>s, <see cref="EntitySystem"/>s and
    /// <see cref="AffectiveSystem"/>s.
    /// </summary>
    public void Dispose() {
        if (isDisposed.Exchange(true))
            return;

        Parallel.ForEach(
            affectiveSystems.Cast<IDisposable>().Concat(systems.Cast<IDisposable>()), (x, _) => x.Dispose()
        );

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Adds and initializes new <paramref name="system"/> to this <see cref="EntityWorld"/>.
    /// </summary>
    /// <param name="system">New, uninitialized <see cref="EntitySystem"/>.</param>
    /// <param name="cycleTime">
    /// Duration in milliseconds of the system execution cycle by <see cref="EntitySchedule"/>.
    /// When <see langword="null"/>, the <see cref="EntitySchedule"/> is not used. Default <see langword="null"/>.
    /// </param>
    public void AddSystem(EntitySystem system, double? cycleTime = null) {
        system.InternalInitialize(this, null);
        if (cycleTime.HasValue)
            system.CycleTime = cycleTime.Value;

        systems.Add(system);
        RegisterArchetypesToSystem(system);
    }

    /// <summary>
    /// Adds and initializes new <paramref name="system"/> to this <see cref="EntityWorld"/>.
    /// </summary>
    /// <param name="system">New, uninitialized <see cref="AffectiveSystem"/>.</param>
    public void AddAffectiveSystem(AffectiveSystem system) {
        system.InternalInitialize(this);
        affectiveSystems.Add(system);

        lock (archetypes) {
            foreach (Archetype archetype in archetypes.Values)
                archetype.RegisterAffectiveSystem(system);
        }
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
        if (!TryGetArchetype(0, out Archetype? archetype))
            archetype = CreateArchetype(0, Array.Empty<(Type type, int size, int affectiveHashCode)>());

        (ArchetypeChunk chunk, nint index) = archetype.TakeRecord();
        Entity entity = new Entity(chunk, index);

        unsafe {
            fixed (byte* ptr = chunk.StorageData) {
                byte* pointer = ptr + index;

                Unsafe.AsRef<EntityInternalComponent>(pointer) = new EntityInternalComponent(entity);
            }
        }

        archetype.InitializeRecord();
        return entity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool TryGetArchetype(int hashCode, [NotNullWhen(true)] out Archetype? archetype) {
        return archetypes.TryGetValue(hashCode, out archetype);
    }

    internal Archetype CreateArchetype(int hashCode, (Type type, int size, int affectiveHashCode)[] components) {
        Archetype? archetype;
        lock (archetypes) {
            if (archetypes.TryGetValue(hashCode, out archetype))
                return archetype;

            archetype = new Archetype(this, hashCode, components);
            archetypes.Add(hashCode, archetype);
        }

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
