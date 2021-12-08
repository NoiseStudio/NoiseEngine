using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace NoiseStudio.JobsAg {
    internal class JobsQueue : IDisposable {

        internal readonly ConcurrentQueue<Job> endQueue = new ConcurrentQueue<Job>();

        private readonly ConcurrentQueue<Job>[] queues;
        private readonly uint[] queuesGaps;
        private readonly JobWorld world;

        private bool works = true;

        public bool IsDisposed { get; private set; }

        public JobsQueue(JobWorld world, uint[]? queues = null) {
            this.world = world;
            works = true;

            if (queues == null) {
                // Default queues
                queues = new uint[] {
                    5_000, // 5 seconds
                    30_000, // 30 seconds
                    120_000, // 2 minutes
                    300_000, // 5 minutes
                    900_000, // 15 minutes
                    3600_000, // 1 hour
                };
            }
            queuesGaps = queues;

            this.queues = new ConcurrentQueue<Job>[queues.Length];
            for (int i = 0; i < this.queues.Length; i++) {
                this.queues[i] = new ConcurrentQueue<Job>();

                Thread thread = new Thread(QueueSortThreadWork);
                thread.Name = $"{nameof(JobsQueue)} sorting thread #{i}";
                thread.Start(i);
            }
        }

        ~JobsQueue() {
            Abort();
        }

        public void Dispose() {
            lock (this) {
                if (IsDisposed)
                    return;

                IsDisposed = true;
            }

            Abort();
            GC.SuppressFinalize(this);
        }

        public void Enqueue(Job job) {
            ulong timeToExecute = job.ExecutionTime.Difference(world.WorldTime);
            for (int i = queuesGaps.Length - 1; i >= 0; i--) {
                if (timeToExecute >= queuesGaps[i]) {
                    queues[i].Enqueue(job);
                    return;
                }
            }
            endQueue.Enqueue(job);
        }

        private void QueueSortThreadWork(object? indexObj) {
            int index = (int)indexObj!;
            ConcurrentQueue<Job> queue = queues[index];
            uint gap = queuesGaps[index];

            int waitTime = (int)Math.Max(gap - 1, 1);
            Queue<Job> jobs = new Queue<Job>();

            while (works) {
                while (queue.TryDequeue(out Job job))
                    jobs.Enqueue(job);
                
                JobTime time = world.WorldTime;
                int count = jobs.Count;
                for (int i = 0; i < count; i++) {
                    Job job = jobs.Dequeue();
                    ulong timeToExecute = job.ExecutionTime.Difference(time);
                    if (timeToExecute > gap) {
                        jobs.Enqueue(job);
                        continue;
                    }

                    bool breaked = false;
                    for (int j = index; j >= 0; j--) {
                        if (timeToExecute >= queuesGaps[index]) {
                            queues[index].Enqueue(job);
                            breaked = true;
                            break;
                        }
                    }
                    if (!breaked)
                        endQueue.Enqueue(job);
                }

                Thread.Sleep(waitTime);
            }
        }

        private void Abort() {
            works = false;
        }

    }
}
