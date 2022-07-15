using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace NoiseEngine.Jobs {
    internal class EntityScheduleWorker : IDisposable {

        private const int DefaultMaxPackageSize = 64;
        private const int DefaultMinPackageSize = 8;

        internal readonly ConcurrentDictionary<int, int> threadIds = new ConcurrentDictionary<int, int>();
        internal readonly int threadIdCount;

        private readonly AutoResetEvent enqueueThreadLocker = new AutoResetEvent(false);
        private readonly ManualResetEventSlim workerThreadsLocker = new ManualResetEventSlim(false);

        private readonly ConcurrentQueue<SchedulePackage> packages = new ConcurrentQueue<SchedulePackage>();
        private readonly ConcurrentList<EntitySystemBase> systems = new ConcurrentList<EntitySystemBase>();
        private readonly ConcurrentHashSet<EntitySystemBase> systemsHashSet = new ConcurrentHashSet<EntitySystemBase>();
        private readonly int threadCount;
        private readonly int minPackageSize;
        private readonly int maxPackageSize;

        private int wakeUpWorkerThreadCount;
        private int activeWorkerThreadCount;
        private bool works = true;

        /// <summary>
        /// Creates new Entity Schedule Worker
        /// </summary>
        /// <param name="threadCount">Number of used threads. When null the number of threads contained in the processor is used.</param>
        /// <param name="maxPackageSize">The maximum size of an UpdateEntity package shared between threads</param>
        /// <param name="minPackageSize">The minimum size of an UpdateEntity package shared between threads</param>
        /// <exception cref="InvalidOperationException">Error when using zero or negative threads and when the minimum package size is greater than the maximum package size</exception>
        public EntityScheduleWorker(int? threadCount = null, int? maxPackageSize = null, int? minPackageSize = null) {
            threadCount ??= Environment.ProcessorCount;
            maxPackageSize ??= DefaultMaxPackageSize;
            minPackageSize ??= DefaultMinPackageSize;

            if (threadCount <= 0)
                throw new InvalidOperationException("The number of threads cannot be zero or negative.");
            if (minPackageSize > maxPackageSize)
                throw new InvalidOperationException("The minimum package size is greater than used maximum package size.");

            this.threadCount = (int)threadCount;
            this.maxPackageSize = (int)maxPackageSize;
            this.minPackageSize = (int)minPackageSize;

            threadIdCount = this.threadCount + 1;
            activeWorkerThreadCount = this.threadCount;

            for (int i = 0; i < this.threadCount; i++) {
                new Thread(ThreadWork) {
                    Name = $"{nameof(EntitySchedule)} worker #{i}"
                }.Start(i);
            }

            new Thread(EnqueueThreadWorker) {
                Name = $"{nameof(EntitySchedule)} enqueue worker"
            }.Start();
        }

        /// <summary>
        /// This <see cref="EntityScheduleWorker"/> will be deactivated and disposed
        /// </summary>
        public void Dispose() {
            works = false;
            Interlocked.Exchange(ref wakeUpWorkerThreadCount, threadCount);

            lock (enqueueThreadLocker) {
                if (!enqueueThreadLocker.SafeWaitHandle.IsClosed)
                    enqueueThreadLocker.Set();
            }

            lock (workerThreadsLocker) {
                if (!workerThreadsLocker.WaitHandle.SafeWaitHandle.IsClosed)
                    workerThreadsLocker.Set();
            }
        }

        internal void AddSystem(EntitySystemBase system) {
            if (!systemsHashSet.Add(system))
                return;

            systems.Add(system);
            enqueueThreadLocker.Set();
        }

        internal void RemoveSystem(EntitySystemBase system) {
            systems.Remove(system);
            systemsHashSet.Remove(system);
        }

        internal bool HasSystem(EntitySystemBase system) {
            return systemsHashSet.Contains(system);
        }

        internal void EnqueuePackages(EntitySystemBase system) {
            EnqueuePackagesWorker(system);
            SignalWorkerThreads();
        }

        private void ThreadWork(object? threadIdObject) {
            int threadId = (int)threadIdObject!;
            threadIds.TryAdd(Environment.CurrentManagedThreadId, threadId + 1);

            while (works) {
                while (packages.TryDequeue(out SchedulePackage package)) {
                    if (package.IsCycleBegin) {
                        package.EntitySystem.InternalUpdate();
                        EnqueuePackagesWorker(package.EntitySystem);
                        package.EntitySystem.ReleaseWork();
                        SignalWorkerThreads();
                        continue;
                    }

                    EntityQueryBase query = package.EntitySystem.query!;
                    if (
                        query.WritableComponents.Count != 0 &&
                        !package.EntityGroup!.TryEnterWriteLock(package.PackageStartIndex)
                    ) {
                        if (packages.Count > 0) {
                            packages.Enqueue(package);
                            continue;
                        }

                        package.EntityGroup.EnterWriteLock(package.PackageStartIndex);
                    }

                    for (int i = package.PackageStartIndex; i < package.PackageEndIndex; i++) {
                        Entity entity = package.EntityGroup!.Entities[i];
                        if (entity != Entity.Empty)
                            package.EntitySystem.InternalUpdateEntity(entity);
                    }

                    if (query.WritableComponents.Count != 0)
                        package.EntityGroup!.ExitWriteLock(package.PackageStartIndex);

                    package.EntityGroup!.ReleaseWork();
                    package.EntitySystem.ReleaseWork();
                }

                workerThreadsLocker.Wait();
                if (Interlocked.Decrement(ref wakeUpWorkerThreadCount) == 0)
                    workerThreadsLocker.Reset();
            }

            if (Interlocked.Decrement(ref activeWorkerThreadCount) == 0) {
                lock (workerThreadsLocker)
                    workerThreadsLocker.Dispose();
            }
        }

        private void EnqueueThreadWorker() {
            while (works) {
                if (systems.Count == 0)
                    enqueueThreadLocker.WaitOne();

                double executionTime = Time.UtcMilliseconds;
                EntitySystemBase[] sortedSystems =
                    systems.OrderByDescending(t => executionTime - t.lastExecutionTime).ToArray();

                bool needToWait = true;
                foreach (EntitySystemBase system in sortedSystems) {
                    double executionTimeDifference = executionTime - system.lastExecutionTime;

                    if (system.cycleTimeWithDelta >= executionTimeDifference)
                        break;

                    if (!system.CheckIfCanExecuteAndOrderWork())
                        continue;

                    packages.Enqueue(new SchedulePackage(system));
                    SignalWorkerThreads();

                    needToWait = false;
                }

                if (needToWait) {
                    EntitySystemBase systemToWait = sortedSystems[0];
                    double executionTimeDifferenceToWait = Time.UtcMilliseconds - systemToWait.lastExecutionTime;
                    int timeToWait = (int)(systemToWait.CycleTime! - executionTimeDifferenceToWait);

                    if (timeToWait > 0)
                        enqueueThreadLocker.WaitOne(timeToWait);
                }
            }

            lock (enqueueThreadLocker)
                enqueueThreadLocker.Dispose();
        }

        private void EnqueuePackagesWorker(EntitySystemBase system) {
            foreach (EntityGroup group in system.query!.groups) {
                group.OrderWorkAndWait();

                int entitiesPerPackage = Math.Clamp(group.EntityCount / threadCount, minPackageSize, maxPackageSize);
                for (int j = 0; j < group.Entities.Count;) {
                    group.OrderWork();
                    system.OrderWork();

                    int endIndex = j + entitiesPerPackage;
                    if (endIndex > group.Entities.Count)
                        endIndex = group.Entities.Count;

                    packages.Enqueue(new SchedulePackage(system, group, j, endIndex));
                    j = endIndex;
                }

                group.ReleaseWork();
            }
        }

        private void SignalWorkerThreads() {
            Interlocked.Exchange(ref wakeUpWorkerThreadCount, threadCount);
            workerThreadsLocker.Set();
        }

    }
}
