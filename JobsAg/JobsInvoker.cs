using System;
using System.Collections.Concurrent;
using System.Threading;

namespace NoiseStudio.JobsAg {
    public class JobsInvoker : IDisposable {

        private static readonly object locker = new object();

        private readonly AutoResetEvent toInvokeAutoResetEvent = new AutoResetEvent(false);
        private readonly ManualResetEvent invokeManualResetEvent = new ManualResetEvent(false);
        private readonly ConcurrentList<JobsQueue> queues = new ConcurrentList<JobsQueue>();
        private readonly ConcurrentQueue<(Job, JobsWorld)> toInvoke = new ConcurrentQueue<(Job, JobsWorld)>();
        private readonly int threadCount;

        private bool works = true;
        private long waitTime = 0;
        private int toInvokeAutoResetEventRelease = 0;

        public static JobsInvoker? Instance { get; private set; }

        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Creates new <see cref="JobsInvoker"/>
        /// </summary>
        /// <param name="threadCount">Number of used threads. When null the number of threads contained in the processor is used.</param>
        /// <exception cref="ArgumentOutOfRangeException">Error when using zero or negative threads</exception>
        public JobsInvoker(int? threadCount = null) {
            if (threadCount == null)
                threadCount = Environment.ProcessorCount;

            if (threadCount <= 0)
                throw new ArgumentOutOfRangeException("The number of threads cannot be zero or negative.");

            this.threadCount = (int)threadCount;

            lock (locker) {
                if (Instance == null)
                    Instance = this;
            }

            Thread thread = new Thread(ToInvokeThreadWork);
            thread.Name = $"{nameof(JobsInvoker)} end queue";
            thread.Start();

            for (int i = 0; i < threadCount; i++) {
                thread = new Thread(InvokeThreadWork);
                thread.Name = $"{nameof(JobsInvoker)} worker #{i}";
                thread.Start();
            }
        }

        ~JobsInvoker() {
            Abort();
        }

        /// <summary>
        /// This <see cref="JobsInvoker"/> will be deactivated and disposed
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

        internal void InvokeJob(Job job, JobsWorld world) {
            toInvoke.Enqueue((job, world));
            invokeManualResetEvent.Set();
        }

        internal void SetToInvokeWaitTime(long waitTime) {
            if (waitTime < this.waitTime) {
                Interlocked.Increment(ref toInvokeAutoResetEventRelease);
                toInvokeAutoResetEvent.Set();
            }
        }

        internal void AddJobsQueue(JobsQueue queue) {
            queues.Add(queue);
        }

        internal void RemoveJobsQueue(JobsQueue queue) {
            queues.Remove(queue);
        }

        private void ToInvokeThreadWork() {
            while (works) {
                Interlocked.Exchange(ref toInvokeAutoResetEventRelease, 0);
                long newWaitTime = long.MaxValue;

                foreach (JobsQueue queue in queues)
                    queue.DequeueToInvoke(ref newWaitTime);

                waitTime = newWaitTime;
                if (waitTime > 0 && toInvokeAutoResetEventRelease != 0) {
                    toInvokeAutoResetEvent.Reset();
                    toInvokeAutoResetEvent.WaitOne((int)waitTime);
                    waitTime = 0;
                }
            }
        }

        private void InvokeThreadWork() {
            while (works) {
                invokeManualResetEvent.WaitOne();

                while (toInvoke.TryDequeue(out (Job job, JobsWorld world) jobObject)) {
                    jobObject.job.Invoke(jobObject.world);
                }

                invokeManualResetEvent.Reset();
            }
        }

        private void Abort() {
            works = false;
        }

    }
}
