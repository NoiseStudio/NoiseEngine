using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NoiseEngine.Jobs {
    public class EntitySchedule : IDisposable {

        public const int DefaultMaxPackageSize = 64;
        public const int DefaultMinPackageSize = 8;

        private static readonly object locker = new object();

        internal readonly ConcurrentDictionary<int, int> threadIds = new ConcurrentDictionary<int, int>();
        internal readonly int threadIdCount;
        
        private readonly ManualResetEvent manualResetEvent = new ManualResetEvent(false);
        private int manualResetEventThreads;
        private readonly object addPackagesLocker = new object();

        private readonly ConcurrentQueue<SchedulePackage> priorityPackages = new ConcurrentQueue<SchedulePackage>();
        private readonly ConcurrentQueue<SchedulePackage> packages = new ConcurrentQueue<SchedulePackage>();
        private readonly List<EntitySystemBase> systems = new List<EntitySystemBase>();
        private readonly HashSet<EntitySystemBase> systemsHashSet = new HashSet<EntitySystemBase>();
        private readonly int threadCount;
        private readonly int minPackageSize;
        private readonly int maxPackageSize;
        private bool works = true;

        public static EntitySchedule? Instance { get; private set; }

        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Creates new Entity Schedule
        /// </summary>
        /// <param name="threadCount">Number of used threads. When null the number of threads contained in the processor is used.</param>
        /// <param name="maxPackageSize">The maximum size of an UpdateEntity package shared between threads</param>
        /// <param name="minPackageSize">The minimum size of an UpdateEntity package shared between threads</param>
        /// <exception cref="InvalidOperationException">Error when using zero or negative threads and when the minimum package size is greater than the maximum package size</exception>
        public EntitySchedule(int? threadCount = null, int? maxPackageSize = null, int? minPackageSize = null) {
            if (threadCount == null)
                threadCount = Environment.ProcessorCount;
            if (maxPackageSize == null)
                maxPackageSize = DefaultMaxPackageSize;
            if (minPackageSize == null)
                minPackageSize = DefaultMinPackageSize;

            if (threadCount <= 0)
                throw new InvalidOperationException("The number of threads cannot be zero or negative.");
            if (minPackageSize > maxPackageSize)
                throw new InvalidOperationException("The minimum package size is greather than used maximum package size.");

            this.threadCount = (int)threadCount;
            this.maxPackageSize = (int)maxPackageSize;
            this.minPackageSize = (int)minPackageSize;

            lock (locker) {
                if (Instance == null)
                    Instance = this;
            }

            threadIdCount = this.threadCount + 1;

            for (int i = 0; i < threadCount; i++) {
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
            lock (systems)
                systems.Add(system);
            lock (systemsHashSet)
                systemsHashSet.Add(system);

            manualResetEventThreads = threadCount;
            manualResetEvent.Set();
        }

        internal void RemoveSystem(EntitySystemBase system) {
            lock (systems)
                systems.Remove(system);
            lock (systemsHashSet)
                systemsHashSet.Remove(system);
        }

        internal bool HasSystem(EntitySystemBase system) {
            return systemsHashSet.Contains(system);
        }

        internal void EnqueuePriorityPackages(EntitySystemBase system) {
            EnqueuePackages(system, priorityPackages);
            SignalWorkerThreads();
        }

        private void ThreadWork(object? threadIdObject) {
            int threadId = (int)threadIdObject!;
            threadIds.TryAdd(Environment.CurrentManagedThreadId, threadId + 1);

            while (works) {
                if (!AddPackages()) {
                    manualResetEvent.WaitOne();
                    if (Interlocked.Decrement(ref manualResetEventThreads) <= 0)
                        manualResetEvent.Reset();
                }

                while (priorityPackages.TryDequeue(out SchedulePackage package) || packages.TryDequeue(out package)) {
                    for (int i = package.PackageStartIndex; i < package.PackageEndIndex; i++) {
                        Entity entity = package.EntityGroup.entities[i];
                        if (entity != Entity.Empty)
                            package.EntitySystem.InternalUpdateEntity(entity);
                    }
                    package.EntityGroup.ReleaseWork();
                    package.EntitySystem.ReleaseWork();
                }
            }
        }

        private bool AddPackages() {
            if (systems.Count == 0 || !Monitor.TryEnter(addPackagesLocker))
                return false;

            while (true) {
                double executionTime = Time.UtcMilliseconds;
                List<EntitySystemBase> sortedSystems;
                lock (systems)
                    sortedSystems = systems.OrderByDescending(t => executionTime - t.lastExecutionTime).ToList();

                bool needToWait = true;
                for (int i = 0; i < sortedSystems.Count; i++) {
                    EntitySystemBase system = sortedSystems[i];
                    double executionTimeDifference = executionTime - system.lastExecutionTime;

                    if (system.cycleTimeWithDelta < executionTimeDifference && system.CanExecute) {
                        EnqueuePackages(system, packages);

                        system.OrderWork();
                        system.InternalUpdate();
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
                    Thread.Sleep(timeToWait);
            }

            SignalWorkerThreads();
            Monitor.Exit(addPackagesLocker);
            return true;
        }

        private void EnqueuePackages(EntitySystemBase system, ConcurrentQueue<SchedulePackage> packages) {
            for (int i = 0; i < system.groups.Count; i++) {
                EntityGroup group = system.groups[i];
                group.Wait();

                int entitiesPerPackage = Math.Clamp(group.entities.Count / threadCount, minPackageSize, maxPackageSize);
                for (int j = 0; j < group.entities.Count;) {
                    group.OrderWork();
                    system.OrderWork();

                    int endIndex = j + entitiesPerPackage;
                    if (endIndex > group.entities.Count)
                        endIndex = group.entities.Count;

                    packages.Enqueue(new SchedulePackage(system, group, j, endIndex));
                    j = endIndex;
                }

                group.ReleaseWork();
            }
        }

        private void SignalWorkerThreads() {
            manualResetEventThreads = threadCount - 1;
            manualResetEvent.Set();
        }

        private void Abort() {
            works = false;
            manualResetEventThreads = int.MaxValue;
            manualResetEvent.Set();
        }

    }
}
