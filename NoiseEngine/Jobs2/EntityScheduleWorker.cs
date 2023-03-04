using NoiseEngine.Collections.Concurrent;
using NoiseEngine.Jobs2.Commands;
using System;
using System.Collections.Concurrent;
using System.Linq;
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
                packages.Enqueue(new SchedulePackage(false, system, chunk, 0, chunk.Count + 1));
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

        try {
            while (work) {
                while (packages.TryDequeue(out executionData)) {
                    if (executionData.IsCycleBegin) {
                        executionData.System.InternalUpdate();
                        EnqueuePackagesWorker(executionData.System);
                        SignalExecutorThreads();
                        executionData.System.ReleaseWork();
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
                                systemCommands
                            );
                        }
                    }
                    EntitySchedule.isScheduleLockThread = false;

                    if (executionData.System.ComponentWriteAccess)
                        locker.ExitWriteLock();
                    else
                        locker.ExitReadLock();
                    executionData.System.ReleaseWork();

                    if (systemCommands.Commands.Count > 0) {
                        new SystemCommandsExecutor(systemCommands.Commands).Invoke();
                        systemCommands.Commands.Clear();
                    }
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
