using NoiseEngine.Collections.Concurrent;
using NoiseEngine.Jobs2.Commands;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
        List<(object?, object?)> changed = new List<(object?, object?)>();

        try {
            while (work) {
                while (packages.TryDequeue(out executionData)) {
                    if (executionData.IsCycleBegin) {
                        EnqueuePackages(executionData.System);
                        continue;
                    }

                    EntityLocker locker = executionData.Chunk!.GetLocker(executionData.StartIndex);
                    if (
                        executionData.System.ComponentWriteAccess ? !locker.TryEnterWriteLock(1) :
                        !locker.TryEnterReadLock(1)
                    ) {
                        packages.Enqueue(executionData);
                        continue;
                    }

                    EntitySchedule.isScheduleLockThread = true;
                    unsafe {
                        fixed (byte* ptr = executionData.Chunk.StorageData) {
                            executionData.System.SystemExecutionInternal(
                                executionData.Chunk, (nint)(executionData.StartIndex + ptr),
                                (nint)(executionData.EndIndex * executionData.Chunk.RecordSize + ptr),
                                systemCommands, changed
                            );

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
                                            observer.Observer, entity, systemCommands.Inner, changedPtr,
                                            executionData.Chunk.Offsets, ref oldValue
                                        );
                                    }
                                }

                                list.Return();
                            }
                        }
                    }
                    EntitySchedule.isScheduleLockThread = false;

                    if (executionData.System.ComponentWriteAccess)
                        locker.ExitWriteLock();
                    else
                        locker.ExitReadLock();
                    executionData.System.ReleaseWork();

                    if (systemCommands.Inner.Commands.Count > 0) {
                        new SystemCommandsExecutor(systemCommands.Inner.Commands).Invoke();
                        systemCommands.Inner.Commands.Clear();
                    }

                    changed.Clear();
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

}
