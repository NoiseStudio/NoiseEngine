using System;
using System.Threading;
using System.Collections.Concurrent;

namespace NoiseStudio.JobsAg {
    public class EntityScheduler {

        public const uint PackageSize = 64;

        private static readonly object locker = new object();

        private readonly ConcurrentQueue<int> packages = new ConcurrentQueue<int>();
        private bool works = true;

        public static EntityScheduler? Instance { get; private set; }

        public EntityScheduler(uint? threadCount = null, uint? packageSize = null) {
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
                thread.Name = $"{nameof(EntityScheduler)} worker #{i}";
                thread.Start();
            }
        }

        ~EntityScheduler() {
            Abort();
        }

        /// <summary>
        /// This <see cref="EntityScheduler"/> will be deactivated
        /// </summary>
        public void Abort() {
            works = false;
        }

        private void ThreadWork() {
            while (works) {

            }
        }

        private void AddPackages() {

        }

    }
}
