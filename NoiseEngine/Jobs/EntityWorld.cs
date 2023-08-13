using NoiseEngine.Collections;
using NoiseEngine.Collections.Concurrent;
using NoiseEngine.Jobs.Commands;
using NoiseEngine.Threading;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace NoiseEngine.Jobs;

public partial class EntityWorld : IDisposable {

    internal readonly List<ChangedObserverContext> changedObservers = new List<ChangedObserverContext>();

    private readonly ConcurrentDictionary<int, Archetype> archetypes = new ConcurrentDictionary<int, Archetype>();
    private readonly ConcurrentList<AffectiveSystem> affectiveSystems = new ConcurrentList<AffectiveSystem>();
    private readonly ConcurrentList<EntitySystem> systems = new ConcurrentList<EntitySystem>();
    private readonly ConcurrentList<WeakReference<EntityQuery>> queries =
        new ConcurrentList<WeakReference<EntityQuery>>();
    private readonly object despawnQueueLocker = new object();

    private ConcurrentQueue<Entity>? despawnQueue;
    private AutoResetEvent? despawnQueueResetEvent;
    private bool despawnQueueThreadWork;
    private AtomicBool isDisposed;

    public EntitySchedule? DefaultSchedule { get; set; } = Application.EntitySchedule;

    public bool IsDisposed => isDisposed;
    public IEnumerable<AffectiveSystem> AffectiveSystems => affectiveSystems;
    public IEnumerable<EntitySystem> Systems => systems;

    internal IEnumerable<Archetype> Archetypes => archetypes.Values;
    internal IEnumerable<WeakReference<EntityQuery>> Queries => queries;

    internal delegate void ChangedObserverInvoker(
        Delegate observer, Entity entity, SystemCommandsInner commands, nint ptr, Dictionary<Type, nint> offsets,
        ref byte oldValue
    );

    /// <summary>
    /// Disposes this <see cref="EntityWorld"/> and it's <see cref="Entity"/>s, <see cref="EntitySystem"/>s and
    /// <see cref="AffectiveSystem"/>s.
    /// </summary>
    public void Dispose() {
        if (isDisposed.Exchange(true))
            return;

        OnDispose();
        OnDisposeInternal();

        archetypes.Clear();
        Parallel.ForEach(
            affectiveSystems.Cast<IDisposable>().Concat(systems.Cast<IDisposable>()), (x, _) => x.Dispose()
        );
        affectiveSystems.Clear();
        systems.Clear();
        queries.Clear();
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
    /// Tries returns any T type <see cref="EntitySystem"/> on this <see cref="EntityWorld"/>.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="EntitySystem"/>.</typeparam>
    /// <param name="system">Founded <see cref="EntitySystem"/> or <see langword="null"/>.</param>
    /// <returns>
    /// Returns <see langword="true"/> when this <see cref="EntityWorld"/> contains T system; otherwise
    /// <see langword="false"/>.
    /// </returns>
    public bool TryGetSystem<T>(out T? system) where T : EntitySystem {
        system = (T?)Systems.FirstOrDefault(x => x.GetType() == typeof(T));
        return system is not null;
    }

    /// <summary>
    /// Returns all T type <see cref="EntitySystem"/> on this <see cref="EntityWorld"/>.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="EntitySystem"/>.</typeparam>
    /// <returns><see cref="IEnumerable{T}"/> with <see cref="EntitySystem"/>s.</returns>
    public IEnumerable<T> GetSystems<T>() where T : EntitySystem {
        return Systems.OfType<T>();
    }

    /// <summary>
    /// Checks if this <see cref="EntityWorld"/> has any T type system.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="EntitySystem"/>.</typeparam>
    /// <returns>
    /// Returns <see langword="true"/> when this <see cref="EntityWorld"/> contains T system; otherwise
    /// <see langword="false"/>.
    /// </returns>
    public bool HasAnySystem<T>() where T : EntitySystem {
        return Systems.Any(x => x.GetType() == typeof(T));
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
        AssertIsNotDisposed();

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
        AssertIsNotDisposed();
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

    /// <summary>
    /// This method is executed when <see cref="Dispose"/> is called.
    /// </summary>
    protected virtual void OnDispose() {
    }

    /// <summary>
    /// This method is executed when <see cref="Dispose"/> is called.
    /// </summary>
    private protected virtual void OnDisposeInternal() {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool TryGetArchetype(int hashCode, [NotNullWhen(true)] out Archetype? archetype) {
        return archetypes.TryGetValue(hashCode, out archetype);
    }

    internal Archetype CreateArchetype(int hashCode, (Type type, int affectiveHashCode)[] components) {
        AssertIsNotDisposed();
        Archetype? archetype;
        lock (archetypes) {
            if (archetypes.TryGetValue(hashCode, out archetype))
                return archetype;

            List<(Type type, int affectiveHashCode)>? list = null;
            int referenceHashCode = hashCode;

            foreach ((Type type, _) in components) {
                foreach (
                    AppendComponentDefaultAttribute attribute in
                    type.GetCustomAttributes<AppendComponentDefaultAttribute>()
                ) {
                    list ??= new List<(Type type, int affectiveHashCode)>(components);
                    foreach (Type t in attribute.Components) {
                        if (list.Any(x => x.type == t))
                            continue;

                        object obj = Activator.CreateInstance(type, false) ?? throw new UnreachableException();

                        int affectiveHashCode;
                        if (obj is IAffectiveComponent affectiveComponent)
                            affectiveHashCode = affectiveComponent.GetAffectiveHashCode();
                        else
                            affectiveHashCode = 0;

                        list.Add((t, affectiveHashCode));
                        referenceHashCode ^= unchecked(t.GetHashCode() + (affectiveHashCode * 16777619));
                    }
                }
            }

            if (list is not null && list.Count != components.Length) {
                if (!TryGetArchetype(referenceHashCode, out archetype)) {
                    archetype = new Archetype(this, referenceHashCode, list.ToArray());
                    archetypes.TryAdd(referenceHashCode, archetype);
                }
            } else {
                archetype = new Archetype(this, hashCode, components);
            }

            archetypes.TryAdd(hashCode, archetype);
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

    internal void RemoveQuery(EntityQuery query) {
        queries.Remove(query.Weak);
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

    private void InitializeQuery(EntityQuery query, IEntityFilter? filter) {
        queries.Add(query.Weak);
        query.Filter = filter;
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
        AddChangedObserverWorkerInner(new ChangedObserverContext(invoker, observer), types);
        EntityObserverLifetime lifetime = new EntityObserverLifetime(this, EntityObserverType.Changed, observer);
        if (IsDisposed) {
            lifetime.Dispose();
            AssertIsNotDisposed();
        }
        return lifetime;
    }

    private void AddChangedObserverWorkerInner(ChangedObserverContext context, List<Type> types) {
        Type[] without = GetObserverWithAndWithoutAttributes(context.Observer.Method, types);
        Type type = types[0];

        lock (changedObservers) {
            if (changedObservers.Contains(context))
                return;
            changedObservers.Add(context);

            foreach (Archetype archetype in archetypes.Values) {
                if (
                    types.Any(x => !archetype.HashCodes.ContainsKey(x)) ||
                    without.Any(archetype.HashCodes.ContainsKey)
                ) {
                    continue;
                }

                ConcurrentDictionary<Type, ChangedObserverContext[]> dictionary = archetype.ChangedObserversLookup;
                if (!dictionary.TryGetValue(type, out ChangedObserverContext[]? contexts))
                    continue;
                if (!dictionary.TryUpdate(type, contexts.Append(context).ToArray(), contexts))
                    throw new UnreachableException();
            }
        }
    }

    private Type[] GetObserverWithAndWithoutAttributes(MethodInfo method, List<Type> types) {
        AssertIsNotDisposed();

        if (types.Count != types.Distinct().Count())
            throw new InvalidOperationException("Duplicate component type in observer.");

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

        return without;
    }

    private void AssertIsNotDisposed() {
        if (IsDisposed)
            throw new ObjectDisposedException(GetType().FullName);
    }

}
