using System;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace NoiseStudio.JobsAg {
    public class EntitySchedule {

        public const int DefaultPackageSize = 64;
        public const int MinimumPackageSize = 8;

        private static readonly object locker = new object();

        private readonly ManualResetEvent manualResetEvent = new ManualResetEvent(false);
        private int manualResetEventThreads;
        private readonly object addPackagesLocker = new object();

        private readonly ConcurrentQueue<SchedulePackage> packages = new ConcurrentQueue<SchedulePackage>();
        private readonly List<EntitySystemBase> systems = new List<EntitySystemBase>();
        private readonly HashSet<EntitySystemBase> systemsHashSet = new HashSet<EntitySystemBase>();
        private readonly int threadCount;
        private readonly int packageSize;
        private bool works = true;

        public static EntitySchedule? Instance { get; private set; }

        public EntitySchedule(int? threadCount = null, int? packageSize = null) {
            if (threadCount == null)
                threadCount = Environment.ProcessorCount;
            if (packageSize == null)
                packageSize = DefaultPackageSize;

            if (threadCount <= 0)
                throw new InvalidOperationException("The number of threads cannot be zero.");
            if (packageSize <= 0)
                throw new InvalidOperationException("The minimum package size is 1.");

            this.threadCount = (int)threadCount;
            this.packageSize = (int)packageSize;

            lock (locker) {
                if (Instance == null)
                    Instance = this;
            }

            for (int i = 0; i < threadCount; i++) {
                Thread thread = new Thread(ThreadWork);
                thread.Name = $"{nameof(EntitySchedule)} worker #{i}";
                thread.Start();
            }
        }

        ~EntitySchedule() {
            Abort();
        }

        /// <summary>
        /// This <see cref="EntitySchedule"/> will be deactivated
        /// </summary>
        public void Abort() {
            works = false;
            manualResetEventThreads = int.MaxValue;
            manualResetEvent.Set();
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

        private void ThreadWork() {
            while (works) {
                if (!AddPackages()) {
                    manualResetEvent.WaitOne();
                    if (Interlocked.Decrement(ref manualResetEventThreads) <= 0)
                        manualResetEvent.Reset();
                }

                while (packages.TryDequeue(out SchedulePackage package)) {
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

                    if (!system.IsWorking) {
                        double executionTimeDifference = executionTime - system.lastExecutionTime;

                        if (system.CycleTime! < executionTimeDifference) {
                            for (int j = 0; j < system.groups.Count; j++) {
                                EntityGroup group = system.groups[j];
                                group.Wait();

                                int entitiesPerPackage = Math.Clamp(group.entities.Count / threadCount, MinimumPackageSize, packageSize);
                                for (int k = 0; k < group.entities.Count;) {
                                    group.OrderWork();
                                    system.OrderWork();

                                    int endIndex = k + entitiesPerPackage;
                                    if (endIndex > group.entities.Count)
                                        endIndex = group.entities.Count;

                                    packages.Enqueue(new SchedulePackage(system, group, k, endIndex));
                                    k = endIndex;
                                }

                                group.ReleaseWork();
                            }

                            system.OrderWork();
                            system.InternalUpdate();
                            system.ReleaseWork();
                            needToWait = false;
                        }
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

            manualResetEventThreads = threadCount - 1;
            manualResetEvent.Set();
            Monitor.Exit(addPackagesLocker);

            return true;
        }

    }
}
