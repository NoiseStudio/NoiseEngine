using NoiseEngine.Collections.Concurrent;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace NoiseEngine.Jobs;

internal class JobsQueue : IDisposable {

    private readonly ConcurrentQueue<Job> endQueue = new ConcurrentQueue<Job>();
    private readonly ConcurrentQueue<Job>[] queues;
    private readonly uint[] queueGaps;
    private readonly Stack<Job> endQueueSwap = new Stack<Job>();
    private readonly ConcurrentHashSet<ulong> jobsToDestroy = new ConcurrentHashSet<ulong>();
    private readonly JobsWorld world;
    private readonly JobsInvoker invoker;

    public bool IsDisposed { get; private set; }

    public JobsQueue(JobsWorld world, JobsInvoker invoker, uint[]? queues = null) {
        this.world = world;
        this.invoker = invoker;

        if (queues is null) {
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
        queueGaps = queues;

        invoker.AddJobsQueue(this);

        this.queues = new ConcurrentQueue<Job>[queues.Length];
        for (int i = 0; i < this.queues.Length; i++) {
            this.queues[i] = new ConcurrentQueue<Job>();

            new Thread(QueueSortThreadWork) {
                Name = $"{nameof(JobsQueue)} sorting thread #{i}"
            }.Start(i);
        }
    }

    public void Dispose() {
        IsDisposed = true;
        invoker.RemoveJobsQueue(this);

        GC.SuppressFinalize(this);
    }

    public void Enqueue(Job job) {
        long timeToExecute = job.ExecutionTime.Difference(world.WorldTime);
        for (int i = queueGaps.Length - 1; i >= 0; i--) {
            if (timeToExecute >= queueGaps[i]) {
                queues[i].Enqueue(job);
                return;
            }
        }

        PrepareJobToInvoke(job, timeToExecute);
    }

    internal void DestroyJob(Job job) {
        jobsToDestroy.Add(job.Id);
    }

    internal void DequeueToInvoke(ref long minimalWaitTime) {
        JobTime time = world.WorldTime;
        while (endQueue.TryDequeue(out Job job)) {
            long timeToExecute = job.ExecutionTime.Difference(time);
            if (timeToExecute > 0) {
                endQueueSwap.Push(job);
                minimalWaitTime = Math.Min(minimalWaitTime, timeToExecute);
                continue;
            }

            if (!jobsToDestroy.Remove(job.Id))
                invoker.InvokeJob(job, world);
        }

        for (int i = 0; i < endQueueSwap.Count; i++)
            endQueue.Enqueue(endQueueSwap.Pop());
    }

    private void QueueSortThreadWork(object? indexObj) {
        int index = (int)indexObj!;
        ConcurrentQueue<Job> queue = queues[index];
        uint gap = queueGaps[index];

        int waitTime = (int)Math.Max(gap - 1, 1);
        Queue<Job> jobs = new Queue<Job>();

        while (!IsDisposed) {
            while (queue.TryDequeue(out Job job))
                jobs.Enqueue(job);

            JobTime time = world.WorldTime;
            int count = jobs.Count;
            for (int i = 0; i < count; i++) {
                Job job = jobs.Dequeue();
                long timeToExecute = job.ExecutionTime.Difference(time);
                if (timeToExecute > gap) {
                    jobs.Enqueue(job);
                    continue;
                }

                if (jobsToDestroy.Remove(job.Id))
                    continue;

                bool breaked = false;
                for (int j = index; j >= 0; j--) {
                    if (timeToExecute >= queueGaps[index]) {
                        queues[index].Enqueue(job);
                        breaked = true;
                        break;
                    }
                }

                if (!breaked)
                    PrepareJobToInvoke(job, timeToExecute);
            }

            Thread.Sleep(waitTime);
        }
    }

    private void PrepareJobToInvoke(Job job, long timeToExecute) {
        if (timeToExecute > 0) {
            endQueue.Enqueue(job);
            invoker.SetToInvokeWaitTime(timeToExecute);
        } else {
            invoker.InvokeJob(job, world);
        }
    }

}
