using System;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace NoiseStudio.JobsAg {
    public class EntitySchedule {

        public const uint PackageSize = 64;

        private static readonly object locker = new object();

        private readonly AutoResetEvent autoResetEvent = new AutoResetEvent(false);
        private readonly object addPackagesLocker = new object();

        private readonly ConcurrentQueue<SchedulePackage> packages = new ConcurrentQueue<SchedulePackage>();
        private readonly List<EntitySystemBase> systems = new List<EntitySystemBase>();
        private bool works = true;

        public static EntitySchedule? Instance { get; private set; }

        public EntitySchedule(uint? threadCount = null, uint? packageSize = null) {
            if (threadCount == null)
                threadCount = (uint)Environment.ProcessorCount;
            if (packageSize == null)
                packageSize = PackageSize;

            if (threadCount == 0)
                throw new InvalidOperationException("The number of threads cannot be zero.");
            if (packageSize == 0)
                throw new InvalidOperationException("The minimum package size is 1.");

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
            autoResetEvent.Set();
        }

        internal void AddSystem(EntitySystemBase system) {
            lock (systems)
                systems.Add(system);
        }

        internal void RemoveSystem(EntitySystemBase system) {
            lock (systems)
                systems.Remove(system);
        }

        private void ThreadWork() {
            while (works) {
                if (!AddPackages())
                    autoResetEvent.WaitOne();

                while (packages.TryDequeue(out SchedulePackage package)) {

                }
            }
        }

        private bool AddPackages() {
            if (!Monitor.TryEnter(addPackagesLocker))
                return false;

            int executionTime = Environment.TickCount;
            List<EntitySystemBase> sortedSystems;
            lock (systems)
                sortedSystems = systems.OrderByDescending(t => executionTime - t.lastExecutionTime).ToList();

            foreach(EntitySystemBase system in sortedSystems) {
                int executionTimeDifference = executionTime - system.lastExecutionTime;
                if (system.CycleTime < executionTimeDifference) {
                    // TODO: Add packages
                    system.InternalUpdate();
                }
            }

            autoResetEvent.Set();
            Monitor.Exit(addPackagesLocker);
            return true;
        }

    }
}
