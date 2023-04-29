using NoiseEngine.Collections.Concurrent;
using NoiseEngine.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NoiseEngine.Jobs2;

public abstract class EntitySystem : IDisposable {

    #region NoiseEngineInternal

    [Obsolete("This struct is internal and is not part of the API. Do not use.")]
    protected struct NoiseEngineInternal_DoNotUse {

        public readonly record struct ExecutionData(nint RecordSize, nint StartIndex, nint EndIndex) {

            private readonly Dictionary<Type, nint> offsets;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ExecutionData(
                ArchetypeChunk chunk, nint startIndex, nint endIndex
            ) : this(chunk.RecordSize, startIndex, endIndex) {
                offsets = chunk.Offsets;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public nint GetOffset<T>() where T : IComponent {
                return offsets[typeof(T)];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe ref T Get<T>(nint index) where T : IComponent {
                return ref Unsafe.AsRef<T>((byte*)index);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Entity? GetInternalComponent(nint index) {
                return Get<EntityInternalComponent>(index).Entity;
            }

        }

        public readonly record struct ComponentUsage(Type Type, bool WriteAccess);

        public ComponentUsage[] UsedComponents { get; set; }
        public bool ComponentWriteAccess { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static ref T NullRef<T>() {
            return ref Unsafe.AsRef<T>((void*)null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ArchetypeHashCode(Entity entity) {
            return entity.chunk!.ArchetypeHashCode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ArchetypeComponentHashCode<T>(Entity entity) {
            return entity.chunk!.HashCodes[typeof(T)];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool CompareAffectiveComponent<T>(
            ref int archetypeHashCode, in T oldValue, in T newValue
        ) where T : IAffectiveComponent<T> {
            if (oldValue.AffectiveEquals(newValue))
                return false;

            archetypeHashCode ^=
                typeof(T).GetHashCode() + IAffectiveComponent.GetAffectiveHashCode(newValue) * 16777619;
            return true;
        }

        /*[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool UpdateComponent<T>(in T oldValue, in T newValue) where T : IComponent {
            if (oldValue is IAffectiveComponent<T> affective) {
                if (affective.AffectiveEquals(newValue)) {
                    if (oldValue is IEquatable<T> equatable) {
                        if (equatable.Equals(newValue))
                            return false;
                    } else {
                        WarnMissingEquatable<T>();
                        if (oldValue.Equals(newValue))
                            return false;
                    }
                }

                return true;
            } else if (oldValue is IEquatable<T> equatable) {
                if (equatable.Equals(newValue))
                    return false;
            } else {
                WarnMissingEquatable<T>();
                if (oldValue.Equals(newValue))
                    return false;
            }

            return false;
        }*/

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static void ChangeArchetype(
            Entity entity, int hashCode, Func<(Type type, int size, int affectiveHashCode)[]> valueFactory
        ) {
            Archetype oldArchetype = entity.chunk!.Archetype;
            (Type type, int size, int affectiveHashCode)[] values = valueFactory();
            Archetype newArchetype = oldArchetype.World.GetArchetype(
                hashCode,
                () => values.UnionBy(entity.chunk!.Archetype.ComponentTypes, x => x.type).ToArray()
            );

            if (ApplicationJitConsts.IsDebugMode && oldArchetype == newArchetype) {
                StringBuilder builder = new StringBuilder("One or more affective component from ");
                foreach ((Type type, _, _) in values)
                    builder.Append(type.FullName).Append(", ");
                builder.Remove(builder.Length - 2, 2);

                builder.Append(" has repeating affective hash code for not comparable AffectiveEquals implementation");

                Log.Warning(builder.ToString());
            }

            // Copy components.
            ArchetypeChunk oldChunk = entity.chunk!;
            (ArchetypeChunk newChunk, nint newIndex) = newArchetype.TakeRecord();

            entity.chunk = newChunk;
            nint oldIndex = entity.index;
            entity.index = newIndex;

            unsafe {
                fixed (byte* dp = newChunk.StorageData) {
                    byte* di = dp + newIndex;
                    fixed (byte* sp = oldChunk.StorageData) {
                        byte* si = sp + oldIndex;

                        foreach ((Type type, int size, _) in newChunk.Archetype.ComponentTypes) {
                            if (!oldChunk.Offsets.TryGetValue(type, out nint oldOffset))
                                throw new UnreachableException();
                            Buffer.MemoryCopy(si + oldOffset, di + newChunk.Offsets[type], size, size);
                        }

                        // Copy internal component.
                        int iSize = Unsafe.SizeOf<EntityInternalComponent>();
                        Buffer.MemoryCopy(si, di, iSize, iSize);

                        // Clear old data.
                        new Span<byte>(si, (int)oldChunk.Archetype.RecordSize).Clear();
                    }
                }
            }

            oldChunk.Archetype.ReleaseRecord(oldChunk, oldIndex);
            newArchetype.InitializeRecord();
        }

        private static void WarnMissingEquatable<T>() {
            if (ApplicationJitConsts.IsDebugMode) {
                Log.Warning(
                    $"Component {typeof(T)} does not implement {nameof(IEquatable<T>)} interface. " +
                    "What affects performance."
                );
            }
        }

    }

    [Obsolete("This field is internal and is not part of the API. Do not use.")]
    protected NoiseEngineInternal_DoNotUse NoiseEngineInternal_DoNotUse_Storage;

    [Obsolete("This method is internal and is not part of the API. Do not use.")]
    protected virtual void NoiseEngineInternal_DoNotUse_Initialize() {
        throw new InvalidOperationException("NoiseEngine.Generator did not generate the code.");
    }

    [Obsolete("This method is internal and is not part of the API. Do not use.")]
    protected virtual void NoiseEngineInternal_DoNotUse_SystemExecution(
        NoiseEngineInternal_DoNotUse.ExecutionData data, SystemCommands commands
    ) {
        throw new InvalidOperationException("NoiseEngine.Generator did not generate the code.");
    }

    #endregion

    internal readonly ConcurrentList<Archetype> archetypes = new ConcurrentList<Archetype>();

    internal long lastExecutionTime = DateTime.UtcNow.Ticks;
    internal long cycleTimeWithDelta;

    private readonly object workLocker = new object();
    private readonly ManualResetEventSlim workResetEvent = new ManualResetEventSlim(true);
    private readonly object scheduleLocker = new object();
    private readonly object enabledLocker = new object();

    private EntityWorld? world;
    private uint ongoingWork;
    private AtomicBool isWorking;
    private AtomicBool isDisposed;
    private bool enabled;
    private EntitySchedule? schedule;
    private double? cycleTime;
    private bool isDoneInitialize;

    public AffectiveSystem? AffectiveSystem { get; private set; }
    public bool IsInitialized => world is not null;
    public bool IsWorking => isWorking;
    public bool IsDisposed => isDisposed;

    public EntityWorld World {
        get {
            if (world is not null)
                return world;

            AssertIsNotDisposed();
            throw new InvalidOperationException("This system is not initialized.");
        }
    }

    public EntitySchedule? Schedule {
        get => schedule;
        set {
            lock (scheduleLocker) {
                if (schedule is not null && CycleTime.HasValue && isDoneInitialize)
                    schedule.Worker.UnregisterSystem(this);

                schedule = value;
                if (value is not null && CycleTime.HasValue && isDoneInitialize)
                    value.Worker.RegisterSystem(this);
            }
        }
    }

    public double? CycleTime {
        get => cycleTime;
        set {
            lock (scheduleLocker) {
                if (Schedule is null) {
                    cycleTime = value;
                    return;
                }

                if (value.HasValue) {
                    if (!cycleTime.HasValue && isDoneInitialize)
                        Schedule.Worker.RegisterSystem(this);
                } else if (cycleTime.HasValue && isDoneInitialize) {
                    Schedule.Worker.UnregisterSystem(this);
                }

                cycleTime = value;
            }
        }
    }

    public bool Enabled {
        get => enabled;
        set {
            lock (enabledLocker) {
                if (enabled != value) {
                    AssertIsNotDisposed();

                    enabled = value;
                    if (value) {
                        OnStart();
                    } else {
                        Wait();
                        OnStop();
                    }
                }
            }
        }
    }

    protected double DeltaTime { get; private set; } = 1;
    protected float DeltaTimeF { get; private set; } = 1;

#pragma warning disable CS0618
    internal bool ComponentWriteAccess => NoiseEngineInternal_DoNotUse_Storage.ComponentWriteAccess;

    private IEnumerable<Type> UsedComponents => NoiseEngineInternal_DoNotUse_Storage.UsedComponents
        .Select(x => x.Type);
#pragma warning restore CS0618

    /// <summary>
    /// Disposes this <see cref="EntitySystem"/>.
    /// </summary>
    public void Dispose() {
        if (AffectiveSystem is not null) {
            throw new InvalidOperationException(
                $"The {ToString()} entity system is child of {AffectiveSystem} affective system and must only be " +
                $"disposed by it."
            );
        }
        InternalDispose();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Tries performs a cycle on this system with using schedule threads.
    /// </summary>
    /// <returns><see langword="true"/> when cycle was enqueued; otherwise <see langword="false"/>.</returns>
    public bool TryExecute() {
        EntitySchedule? schedule = Schedule;
        if (schedule is null || !TryOrderWork())
            return false;
        schedule.Worker.EnqueueCycleBegin(this);
        return true;
    }

    /// <summary>
    /// Tries performs a cycle on this system with using schedule threads. And wait to end.
    /// </summary>
    /// <returns><see langword="true"/> when cycle was executed; otherwise <see langword="false"/>.</returns>
    public bool TryExecuteAndWait() {
        EntitySchedule? schedule = Schedule;
        if (schedule is null || !TryOrderWork())
            return false;
        schedule.Worker.EnqueuePackages(this);
        Wait();
        return true;
    }

    /// <summary>
    /// Performs a cycle on this system with using schedule threads.
    /// </summary>
    public void Execute() {
        EntitySchedule? schedule = GetEntityScheduleOrThrowException();
        AssertCouldExecute();

        Task.Run(() => {
            WaitWhenCanExecuteAndOrderWork();
            schedule.Worker.EnqueuePackages(this);
        });
    }

    /// <summary>
    /// Performs a cycle on this system with using schedule threads. And wait to end.
    /// </summary>
    public void ExecuteAndWait() {
        EntitySchedule? schedule = GetEntityScheduleOrThrowException();
        AssertCouldExecute();

        WaitWhenCanExecuteAndOrderWork();
        schedule.Worker.EnqueuePackages(this);

        Wait();
    }

    /// <summary>
    /// Blocks the current thread until the cycle completes.
    /// </summary>
    public void Wait() {
        workResetEvent.Wait();
    }

    internal void InternalDispose() {
        if (isDisposed.Exchange(true))
            return;

        Schedule = null;
        lock (enabledLocker) {
            Wait();
            if (enabled) {
                enabled = false;
                OnStop();
            }
            OnDispose();
        }

        World.RemoveSystem(this);
        world = null;
    }

    internal void InternalInitialize(EntityWorld world, AffectiveSystem? affectiveSystem) {
        AssertIsNotDisposed();
        if (Interlocked.CompareExchange(ref this.world, world, null) is not null)
            throw new InvalidOperationException("System is already initialized.");

        AffectiveSystem = affectiveSystem;

#pragma warning disable CS0618
        NoiseEngineInternal_DoNotUse_Initialize();
#pragma warning restore CS0618
        OnInitialize();
        Enabled = true;

        lock (scheduleLocker) {
            isDoneInitialize = true;
            Schedule ??= world.DefaultSchedule;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SystemExecutionInternal(
        ArchetypeChunk chunk, nint startPointer, nint endPointer, SystemCommands systemCommands
    ) {
#pragma warning disable CS0618
        NoiseEngineInternal_DoNotUse_SystemExecution(new NoiseEngineInternal_DoNotUse.ExecutionData(
            chunk, startPointer, endPointer
        ), systemCommands);
#pragma warning restore CS0618
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void InternalUpdate() {
        long executionTime = DateTime.UtcNow.Ticks;

        long difference = executionTime - lastExecutionTime;
        DeltaTime = difference / (double)TimeSpan.TicksPerSecond;
        DeltaTimeF = (float)DeltaTime;

        double? cycleTime = CycleTime;
        if (cycleTime.HasValue) {
            long cycleTimeInTicks = (long)(cycleTime.Value * TimeSpan.TicksPerMillisecond);
            cycleTimeWithDelta = cycleTimeInTicks - (difference - cycleTimeInTicks);
        }

        lastExecutionTime = executionTime;
        OnUpdate();
    }

    internal void RegisterArchetype(Archetype archetype) {
        foreach (Type type in UsedComponents) {
            if (!archetype.Offsets.ContainsKey(type))
                return;
        }

        lock (archetypes) {
            if (!archetypes.Contains(archetype))
                archetypes.Add(archetype);
        }
    }

    internal void OrderWork() {
        if (Interlocked.Increment(ref ongoingWork) != 1)
            return;

        lock (workLocker) {
            isWorking = true;
            workResetEvent.Reset();
        }
    }

    internal void ReleaseWork() {
        if (Interlocked.Decrement(ref ongoingWork) != 0)
            return;

        OnLateUpdate();
        lock (workLocker) {
            isWorking = false;
            workResetEvent.Set();
        }
    }

    internal bool TryOrderWork() {
        if (!Enabled || IsDisposed || isWorking.Exchange(true))
            return false;

        OrderWork();
        return true;
    }

    /// <summary>
    /// This method is executed when this system is initializing.
    /// </summary>
    protected virtual void OnInitialize() {
    }

    /// <summary>
    /// This method is executed when this system is enabling.
    /// </summary>
    protected virtual void OnStart() {
    }

    /// <summary>
    /// This method is executed on begin of every cycle of this system.
    /// </summary>
    protected virtual void OnUpdate() {
    }

    /// <summary>
    /// This method is executed on end of every cycle of this system.
    /// </summary>
    protected virtual void OnLateUpdate() {
    }

    /// <summary>
    /// This method is executed when this system is disabling.
    /// </summary>
    protected virtual void OnStop() {
    }

    /// <summary>
    /// This method is executed when this system is destroying.
    /// </summary>
    protected virtual void OnDispose() {
    }

    private void WaitWhenCanExecuteAndOrderWork() {
        while (!TryOrderWork()) {
            AssertCouldExecute();
            Wait();
        }
    }

    private void AssertCouldExecute() {
        AssertIsNotDisposed();
        if (!Enabled)
            throw new InvalidOperationException($"The {ToString()} entity system is disabled.");
    }

    private void AssertIsNotDisposed() {
        if (isDisposed)
            throw new ObjectDisposedException(ToString(), "Entity system is disposed.");
    }

    private EntitySchedule GetEntityScheduleOrThrowException() {
        return Schedule ?? throw new InvalidOperationException(
            $"{nameof(EntitySchedule)} assigned to this {ToString()} is null."
        );
    }

}
