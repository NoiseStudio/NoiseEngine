using NoiseEngine.Collections.Concurrent;
using NoiseEngine.Jobs2.Commands;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace NoiseEngine.Jobs2;

internal class EntityScheduleWorker : IDisposable {

    private readonly ConcurrentList<EntitySystem> systems = new ConcurrentList<EntitySystem>();
    private readonly ConcurrentQueue<SchedulePackage> packages = new ConcurrentQueue<SchedulePackage>();
    private readonly AutoResetEvent enqueueThreadLocker = new AutoResetEvent(false);
    private readonly ManualResetEventSlim executorThreadsResetEvent = new ManualResetEventSlim(false);

    private bool work = true;
    private int wakeUpExecutorThreadCount;
    private int activeExecutorThreadCount;

    public int ThreadCount { get; }

    public EntityScheduleWorker(int? threadCount) {
        if (threadCount <= 0) {
            throw new ArgumentOutOfRangeException(
                nameof(threadCount), "The number of threads cannot be zero or negative."
            );
        }

        ThreadCount = threadCount ?? Environment.ProcessorCount;
        activeExecutorThreadCount = ThreadCount;

        for (int i = 0; i < ThreadCount; i++) {
            new Thread(ExecutorThreadWork) {
                Name = $"{nameof(EntitySchedule)} worker #{i}",
                IsBackground = true
            }.Start();
        }

        new Thread(EnqueueThreadWork) {
            Name = $"{nameof(EntitySchedule)} enqueue thread",
            IsBackground = true
        }.Start();
    }

    public void Dispose() {
        work = false;
    }

    public void RegisterSystem(EntitySystem system) {
        lock (systems) {
            if (!systems.Contains(system))
                systems.Add(system);
        }

        enqueueThreadLocker.Set();
    }

    public void UnregisterSystem(EntitySystem system) {
        systems.Remove(system);
    }

    public void EnqueueCycleBegin(EntitySystem system) {
        packages.Enqueue(new SchedulePackage(true, system, null, 0, 0));
        SignalExecutorThreads();
    }

    public void EnqueuePackages(EntitySystem system) {
        system.InternalUpdate();
        EnqueuePackagesWorker(system);
        SignalExecutorThreads();
        system.ReleaseWork();
    }

    private void SignalExecutorThreads() {
        Interlocked.Exchange(ref wakeUpExecutorThreadCount, ThreadCount);
        executorThreadsResetEvent.Set();
    }

    private void EnqueuePackagesWorker(EntitySystem system) {
        foreach (Archetype archetype in system.archetypes) {
            foreach (ArchetypeChunk chunk in archetype.chunks) {
                if (chunk.Count < 0)
                    continue;

                system.OrderWork();
                packages.Enqueue(new SchedulePackage(false, system, chunk, 0, chunk.Count));
            }
        }
    }

    private void EnqueueThreadWork() {
        while (work) {
            if (systems.Count == 0)
                enqueueThreadLocker.WaitOne();

            long executionTime = DateTime.UtcNow.Ticks;
            EntitySystem[] sortedSystems =
                systems.OrderByDescending(x => executionTime - x.lastExecutionTime).ToArray();

            if (sortedSystems.Length == 0)
                continue;

            bool needToWait = true;
            foreach (EntitySystem system in sortedSystems) {
                double executionTimeDifference = executionTime - system.lastExecutionTime;
                if (system.cycleTimeWithDelta >= executionTimeDifference)
                    break;

                if (!system.TryOrderWork())
                    continue;

                packages.Enqueue(new SchedulePackage(true, system, null, 0, 0));
                SignalExecutorThreads();

                needToWait = false;
            }

            if (needToWait) {
                EntitySystem systemToWait = sortedSystems[0];
                double? cycleTime = systemToWait.CycleTime;
                if (!cycleTime.HasValue)
                    continue;

                double executionTimeDifferenceToWait =
                    (DateTime.UtcNow.Ticks - systemToWait.lastExecutionTime) / (double)TimeSpan.TicksPerMillisecond;
                int timeToWait = (int)(systemToWait.CycleTime! - executionTimeDifferenceToWait);

                if (timeToWait > 0)
                    enqueueThreadLocker.WaitOne(timeToWait);
            }
        }

        enqueueThreadLocker.Dispose();
    }

    private void ExecutorThreadWork() {
        ManualResetEventSlim executorThreadsResetEvent = this.executorThreadsResetEvent;
        SchedulePackage executionData;
        SystemCommands systemCommands = new SystemCommands();
        List<(nint, int)> changeArchetype = new List<(nint, int)>();
        List<(object?, object?)> changed = new List<(object?, object?)>();

        try {
            while (work) {
                while (packages.TryDequeue(out executionData)) {
                    if (executionData.IsCycleBegin) {
                        EnqueuePackages(executionData.System);
                        continue;
                    }

                    EntityLocker locker = executionData.Chunk!.GetLocker();
                    if (
                        executionData.System.ComponentWriteAccess ? !locker.TryEnterWriteLock(1) :
                        !locker.TryEnterReadLock(1)
                    ) {
                        packages.Enqueue(executionData);
                        continue;
                    }
                    bool lockedWrite = executionData.System.ComponentWriteAccess;

                    EntitySchedule.isScheduleLockThread = true;
                    unsafe {
                        fixed (byte* ptr = executionData.Chunk.StorageData) {
                            executionData.System.SystemExecutionInternal(
                                executionData.Chunk, (nint)(executionData.StartIndex + ptr),
                                (nint)(executionData.EndIndex * executionData.Chunk.RecordSize + ptr),
                                systemCommands, changeArchetype, changed
                            );

                            NotifyChanged(changed, executionData.Chunk.Offsets, systemCommands.Inner);
                            ChangeArchetype(executionData.Chunk, changeArchetype);
                        }
                    }
                    EntitySchedule.isScheduleLockThread = false;

                    if (lockedWrite)
                        locker.ExitWriteLock();
                    else
                        locker.ExitReadLock();
                    executionData.System.ReleaseWork();

                    if (systemCommands.Inner.Commands.Count > 0) {
                        new SystemCommandsExecutor(systemCommands.Inner.Commands).Invoke();
                        systemCommands.Inner.Commands.Clear();
                    }

                    changed.Clear();
                    changeArchetype.Clear();
                }

                executorThreadsResetEvent.Wait();
                if (Interlocked.Decrement(ref wakeUpExecutorThreadCount) == 0)
                    executorThreadsResetEvent.Reset();
            }
        } catch (Exception exception) {
            Log.Error($"Thread terminated due to an exception. {exception}");
            throw;
        } finally {
            if (Interlocked.Decrement(ref activeExecutorThreadCount) == 0)
                executorThreadsResetEvent.Dispose();
        }
    }

    private unsafe void NotifyChanged(
        List<(object?, object?)> changed, Dictionary<Type, nint> offsets, SystemCommandsInner commandsInner
    ) {
        foreach ((object? observersObject, object? listObject) in changed) {
            ChangedObserverContext[] observers =
                Unsafe.As<ChangedObserverContext[]>(observersObject)!;
            ChangedList list = Unsafe.As<ChangedList>(listObject)!;

            Span<byte> buffer = MemoryMarshal.CreateSpan(
                ref Unsafe.As<byte[]>(list.buffer)[0], list.size * list.count
            );

            for (int i = 0; i < buffer.Length; i += list.size) {
                nint changedPtr = Unsafe.ReadUnaligned<nint>(ref buffer[i]);
                Entity entity = Unsafe.ReadUnaligned<EntityInternalComponent>(
                    (void*)changedPtr
                ).Entity!;
                ref byte oldValue = ref buffer[i + Unsafe.SizeOf<nint>()];

                foreach (ChangedObserverContext observer in observers) {
                    observer.Invoker.Invoke(
                        observer.Observer, entity, commandsInner, changedPtr, offsets, ref oldValue
                    );
                }
            }

            list.Return();
        }
    }

    private unsafe void ChangeArchetype(ArchetypeChunk oldChunk, List<(nint, int)> changeArchetype) {
        EntityWorld world = oldChunk.Archetype.World;
        foreach ((nint ptr, int hashCode) in changeArchetype) {
            if (!world.TryGetArchetype(hashCode, out Archetype? newArchetype)) {
                (Type, int, int)[] componentTypes = ((Type, int, int)[])oldChunk.Archetype.ComponentTypes.Clone();

                for (int i = 0; i < componentTypes.Length; i++) {
                    (Type type, int size, int affectiveHashCode) = componentTypes[i];
                    if (affectiveHashCode == 0 && !typeof(IAffectiveComponent).IsAssignableFrom(type))
                        continue;

                    componentTypes[i] = (type, size, ((IAffectiveComponent)oldChunk.ReadComponentBoxed(
                        type, size, ptr + oldChunk.Offsets[type]
                    )).GetAffectiveHashCode());
                }

                newArchetype = world.CreateArchetype(hashCode, componentTypes);
            }

            if (ApplicationJitConsts.IsDebugMode && oldChunk.Archetype == newArchetype) {
                StringBuilder builder = new StringBuilder("One or more affective component from ");
                foreach (
                    Type type in newArchetype.ComponentTypes
                        .Where(x => x.affectiveHashCode != 0 || typeof(IAffectiveComponent).IsAssignableFrom(x.type))
                        .Select(x => x.type)
                ) {
                    builder.Append(type.FullName).Append(", ");
                }
                builder.Remove(builder.Length - 2, 2);

                builder.Append(" has repeating affective hash code for not comparable AffectiveEquals implementation");

                Log.Warning(builder.ToString());
            }

            Entity entity = Unsafe.ReadUnaligned<EntityInternalComponent>((void*)ptr).Entity!;
            (ArchetypeChunk newChunk, nint newIndex) = newArchetype.TakeRecord();

            entity.chunk = newChunk;
            nint oldIndex = entity.index;
            entity.index = newIndex;

            fixed (byte* dp = newChunk.StorageData) {
                byte* di = dp + newIndex;
                int iSize = Unsafe.SizeOf<EntityInternalComponent>();

                // Copy components.
                nint size = newArchetype.RecordSize - iSize;
                Buffer.MemoryCopy((void*)(ptr + iSize), di + iSize, size, size);

                // Copy internal component.
                Buffer.MemoryCopy((void*)ptr, di, iSize, iSize);

                // Clear old data.
                new Span<byte>((void*)ptr, (int)oldChunk.Archetype.RecordSize).Clear();
            }

            oldChunk.Archetype.ReleaseRecord(oldChunk, oldIndex);
            newArchetype.InitializeRecord();
        }
    }

}
