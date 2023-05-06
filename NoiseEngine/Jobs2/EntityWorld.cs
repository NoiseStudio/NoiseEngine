using NoiseEngine.Collections;
using NoiseEngine.Collections.Concurrent;
using NoiseEngine.Jobs2.Commands;
using NoiseEngine.Threading;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace NoiseEngine.Jobs2;

public partial class EntityWorld : IDisposable {

    internal readonly List<ChangedObserverContext> changedObservers = new List<ChangedObserverContext>();

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

    internal IEnumerable<Archetype> Archetypes => archetypes.Values;

    internal delegate void ChangedObserverInvoker(
        Delegate observer, Entity entity, SystemCommandsInner commands, nint ptr, Dictionary<Type, nint> offsets,
        ref byte oldValue
    );

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

    public void AddObserver<T1>(Observers.ChangedObserverT1<T1> observer)
        where T1 : IComponent
    {
        AddChangedObserverWorker(Observers.ChangedObserverT1Invoker<T1>, observer, new List<Type> {
            typeof(T1)
        });
    }

    public void AddObserver<T1, T2>(Observers.ChangedObserverT2C<T1, T2> observer)
        where T1 : IComponent
        where T2 : IComponent
    {
        AddChangedObserverWorker(Observers.ChangedObserverT2CInvoker<T1, T2>, observer, new List<Type> {
            typeof(T1), typeof(T2)
        });
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

    private void AddChangedObserverWorker(ChangedObserverInvoker invoker, Delegate observer, List<Type> types) {
        AddObserverWorker(0, new ChangedObserverContext(invoker, observer), changedObservers, types);
    }

    private void AddObserverWorker(
        int mode, ChangedObserverContext context, List<ChangedObserverContext> observers, List<Type> types
    ) {
        if (types.Count != types.Distinct().Count())
            throw new InvalidOperationException("Duplicate component type in observer.");

        MethodInfo method = context.Observer.Method;
        types.AddRange(method.GetCustomAttributes<WithAttribute>().SelectMany(x => x.Components));
        if (types.Count != types.Distinct().Count())
            throw new InvalidOperationException($"Duplicate component type in {nameof(WithAttribute)}.");

        Type[] without = method.GetCustomAttributes<WithoutAttribute>().SelectMany(x => x.Components).ToArray();
        if (without.Any(types.Contains)) {
            throw new InvalidOperationException(
                $"{nameof(WithoutAttribute)} contains conflicting types with other requirements."
            );
        }
        if (without.Length != without.Distinct().Count())
            throw new InvalidOperationException($"Duplicate component type in {nameof(WithoutAttribute)}.");

        Type type = types[0];
        lock (observers) {
            if (observers.Contains(context))
                return;
            observers.Add(context);

            foreach (Archetype archetype in archetypes.Values) {
                if (
                    types.Any(x => !archetype.HashCodes.ContainsKey(x)) ||
                    without.Any(archetype.HashCodes.ContainsKey)
                ) {
                    continue;
                }

                ConcurrentDictionary<Type, ChangedObserverContext[]> dictionary = mode switch {
                    0 => archetype.ChangedObserversLookup,
                    _ => throw new NotImplementedException(),
                };

                if (!dictionary.TryGetValue(type, out ChangedObserverContext[]? contexts))
                    continue;
                if (!dictionary.TryUpdate(type, contexts.Append(context).ToArray(), contexts))
                    throw new UnreachableException();
            }
        }
    }

}
