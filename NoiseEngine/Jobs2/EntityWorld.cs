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

        archetypes.Clear();
        Parallel.ForEach(
            affectiveSystems.Cast<IDisposable>().Concat(systems.Cast<IDisposable>()), (x, _) => x.Dispose()
        );
        affectiveSystems.Clear();
        systems.Clear();
        changedObservers.Clear();

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
        AssertIsNotDisposed();

        system.InternalInitialize(this, null);
        if (cycleTime.HasValue)
            system.CycleTime = cycleTime.Value;

        systems.Add(system);
        RegisterArchetypesToSystem(system);

        if (IsDisposed) {
            system.Dispose();
            AssertIsNotDisposed();
        }
    }

    /// <summary>
    /// Adds and initializes new <paramref name="system"/> to this <see cref="EntityWorld"/>.
    /// </summary>
    /// <param name="system">New, uninitialized <see cref="AffectiveSystem"/>.</param>
    public void AddAffectiveSystem(AffectiveSystem system) {
        AssertIsNotDisposed();

        system.InternalInitialize(this);
        affectiveSystems.Add(system);

        lock (archetypes) {
            foreach (Archetype archetype in archetypes.Values)
                archetype.RegisterAffectiveSystem(system);
        }

        if (IsDisposed) {
            system.Dispose();
            AssertIsNotDisposed();
        }
    }

    /// <summary>
    /// Adds <paramref name="observer"/> to this <see cref="EntityWorld"/> as changed observer.
    /// </summary>
    /// <remarks>
    /// If a component required by the <paramref name="observer"/> is removed within the scope of a single component
    /// set change, the <paramref name="observer"/> will not be invoked.
    /// </remarks>
    /// <typeparam name="T1"></typeparam>
    /// <param name="observer">Changed Entity Observer.</param>
    /// <returns>New <see cref="EntityObserverLifetime"/> used to disposing this <paramref name="observer"/>.</returns>
    public EntityObserverLifetime AddObserver<T1>(Observers.ChangedObserverT1<T1> observer)
        where T1 : IComponent
    {
        return AddChangedObserverWorker(Observers.ChangedObserverT1Invoker<T1>, observer, new List<Type> {
            typeof(T1)
        });
    }

    public EntityObserverLifetime AddObserver<T1, T2>(Observers.ChangedObserverT2C<T1, T2> observer)
        where T1 : IComponent
        where T2 : IComponent
    {
        return AddChangedObserverWorker(Observers.ChangedObserverT2CInvoker<T1, T2>, observer, new List<Type> {
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
        AssertIsNotDisposed();
        Archetype? archetype;
        lock (archetypes) {
            if (archetypes.TryGetValue(hashCode, out archetype))
                return archetype;

            archetype = new Archetype(this, hashCode, components);
            archetypes.Add(hashCode, archetype);
        }

        archetype.Initialize();
        AssertIsNotDisposed();
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

    internal void RemoveChangedObserver(Delegate observer) {
        lock (changedObservers) {
            ChangedObserverContext? context = null;
            for (int i = 0; i < changedObservers.Count; i++) {
                if (changedObservers[i].Observer != observer)
                    continue;

                context = changedObservers[i];
                changedObservers.RemoveAt(i);
                break;
            }

            if (context.HasValue) {
                ParameterInfo[] parameters = observer.Method.GetParameters();
                int skip = parameters[1].ParameterType == typeof(SystemCommands) ? 2 : 1;
                Type type = parameters[skip].ParameterType.GenericTypeArguments[0];

                foreach (Archetype archetype in Archetypes) {
                    if (archetype.ChangedObserversLookup.TryGetValue(type, out ChangedObserverContext[]? contexts))
                        archetype.ChangedObserversLookup[type] = contexts.Where(x => x != context).ToArray();
                }
            }
        }
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

    private EntityObserverLifetime AddChangedObserverWorker(
        ChangedObserverInvoker invoker, Delegate observer, List<Type> types
    ) {
        AddObserverWorker(
            EntityObserverType.Changed, new ChangedObserverContext(invoker, observer), changedObservers, types
        );
        return new EntityObserverLifetime(this, EntityObserverType.Changed, observer);
    }

    private void AddObserverWorker(
        EntityObserverType observerType, ChangedObserverContext context, List<ChangedObserverContext> observers,
        List<Type> types
    ) {
        AssertIsNotDisposed();

        if (observers.Contains(context))
            return;
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

                ConcurrentDictionary<Type, ChangedObserverContext[]> dictionary = observerType switch {
                    EntityObserverType.Changed => archetype.ChangedObserversLookup,
                    _ => throw new NotImplementedException(),
                };

                if (!dictionary.TryGetValue(type, out ChangedObserverContext[]? contexts))
                    continue;
                if (!dictionary.TryUpdate(type, contexts.Append(context).ToArray(), contexts))
                    throw new UnreachableException();
            }
        }
    }

    private void AssertIsNotDisposed() {
        if (IsDisposed)
            throw new ObjectDisposedException(GetType().FullName);
    }

}
