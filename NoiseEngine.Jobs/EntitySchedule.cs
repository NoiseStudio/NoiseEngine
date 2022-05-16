using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NoiseEngine.Jobs {
    public class EntitySchedule : IDisposable {

        private const int DefaultMaxPackageSize = 64;
        private const int DefaultMinPackageSize = 8;

        internal readonly ConcurrentDictionary<int, int> threadIds = new ConcurrentDictionary<int, int>();
        internal readonly int threadIdCount;

        private readonly IReadOnlyList<AutoResetEvent> autoResetEvents;
        private readonly object addPackagesLocker = new object();

        private readonly ConcurrentQueue<SchedulePackage> packages = new ConcurrentQueue<SchedulePackage>();
        private readonly ConcurrentList<EntitySystemBase> systems = new ConcurrentList<EntitySystemBase>();
        private readonly ConcurrentHashSet<EntitySystemBase> systemsHashSet = new ConcurrentHashSet<EntitySystemBase>();
        private readonly int threadCount;
        private readonly int minPackageSize;
        private readonly int maxPackageSize;

        private int nextThreadToSignal;
        private bool works = true;

        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Creates new Entity Schedule
        /// </summary>
        /// <param name="threadCount">Number of used threads. When null the number of threads contained in the processor is used.</param>
        /// <param name="maxPackageSize">The maximum size of an UpdateEntity package shared between threads</param>
        /// <param name="minPackageSize">The minimum size of an UpdateEntity package shared between threads</param>
        /// <exception cref="InvalidOperationException">Error when using zero or negative threads and when the minimum package size is greater than the maximum package size</exception>
        public EntitySchedule(int? threadCount = null, int? maxPackageSize = null, int? minPackageSize = null) {
            threadCount ??= Environment.ProcessorCount;
            maxPackageSize ??= DefaultMaxPackageSize;
            minPackageSize ??= DefaultMinPackageSize;

            if (threadCount <= 0)
                throw new InvalidOperationException("The number of threads cannot be zero or negative.");
            if (minPackageSize > maxPackageSize)
                throw new InvalidOperationException("The minimum package size is greather than used maximum package size.");

            this.threadCount = (int)threadCount;
            this.maxPackageSize = (int)maxPackageSize;
            this.minPackageSize = (int)minPackageSize;

            threadIdCount = this.threadCount + 1;

            AutoResetEvent[] autoResetEvents = new AutoResetEvent[this.threadCount];
            this.autoResetEvents = autoResetEvents;

            for (int i = 0; i < this.threadCount; i++) {
                autoResetEvents[i] = new AutoResetEvent(false);

                Thread thread = new Thread(ThreadWork);
                thread.Name = $"{nameof(EntitySchedule)} worker #{i}";
                thread.Start(i);
            }
        }

        ~EntitySchedule() {
            Abort();
        }

        /// <summary>
        /// This <see cref="EntitySchedule"/> will be deactivated and disposed
        /// </summary>
        public void Dispose() {
            lock (this) {
                if (IsDisposed)
                    return;

                IsDisposed = true;
            }

            Abort();
            GC.SuppressFinalize(this);
        }

        internal void AddSystem(EntitySystemBase system) {
            if (!systemsHashSet.Add(system))
                return;

            systems.Add(system);
            SignalWorkerThread();
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

            AutoResetEvent autoResetEvent = autoResetEvents[threadId];

            while (works) {
                if (!AddPackages(autoResetEvent)) {
                    autoResetEvent.WaitOne();
                }

                while (packages.TryDequeue(out SchedulePackage package)) {
                    EntityQueryBase query = package.EntitySystem.query!;
                    if (!query.IsReadOnly && !package.EntityGroup.TryEnterWriteLock(package.PackageStartIndex)) {
                        if (packages.Count > 0) {
                            packages.Enqueue(package);
                            continue;
                        }

                        package.EntityGroup.EnterWriteLock(package.PackageStartIndex);
                    }

                    for (int i = package.PackageStartIndex; i < package.PackageEndIndex; i++) {
                        Entity entity = package.EntityGroup.Entities[i];
                        if (entity != Entity.Empty)
                            package.EntitySystem.InternalUpdateEntity(entity);
                    }

                    if (!query.IsReadOnly)
                        package.EntityGroup.ExitWriteLock(package.PackageStartIndex);

                    package.EntityGroup.ReleaseWork();
                    package.EntitySystem.ReleaseWork();
                }
            }
        }

        private bool AddPackages(AutoResetEvent autoResetEvent) {
            if (systems.Count == 0 || !Monitor.TryEnter(addPackagesLocker))
                return false;

            while (true) {
                double executionTime = Time.UtcMilliseconds;
                List<EntitySystemBase> sortedSystems =
                    systems.OrderByDescending(t => executionTime - t.lastExecutionTime).ToList();

                bool needToWait = true;
                for (int i = 0; i < sortedSystems.Count; i++) {
                    EntitySystemBase system = sortedSystems[i];
                    double executionTimeDifference = executionTime - system.lastExecutionTime;

                    if (system.cycleTimeWithDelta < executionTimeDifference && system.CheckIfCanExecuteAndOrderWork()) {
                        system.InternalUpdate();
                        EnqueuePackagesWorker(system);
                        system.ReleaseWork();
                        needToWait = false;
                    }
                }

                if (!needToWait || sortedSystems.Count == 0)
                    break;

                EntitySystemBase systemToWait = sortedSystems[0];
                double executionTimeDifferenceToWait = Time.UtcMilliseconds - systemToWait.lastExecutionTime;
                int timeToWait = (int)(systemToWait.CycleTime! - executionTimeDifferenceToWait);

                if (timeToWait > 0)
                    autoResetEvent.WaitOne(timeToWait);
            }

            SignalWorkerThreads();
            Monitor.Exit(addPackagesLocker);
            return true;
        }

        private void EnqueuePackagesWorker(EntitySystemBase system) {
            foreach (EntityGroup group in system.query!.groups) {
                group.OrderWorkAndWait();

                int entitiesPerPackage = Math.Clamp(group.Entities.Count / threadCount, minPackageSize, maxPackageSize);
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
            int signalThreadsCount = Math.Min(packages.Count, threadCount);
            for (int i = 0; i < signalThreadsCount; i++)
                SignalWorkerThread();
        }

        private void SignalWorkerThread() {
            autoResetEvents[Interlocked.Increment(ref nextThreadToSignal) % threadCount].Set();
        }

        private void Abort() {
            works = false;

            for (int i = 0; i < threadCount; i++)
                SignalWorkerThread();
        }

    }
}
