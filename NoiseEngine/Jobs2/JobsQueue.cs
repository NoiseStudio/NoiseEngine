using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace NoiseEngine.Jobs2;

internal sealed class JobsQueue : IDisposable {

    private readonly long startRealTime;

    private readonly ConcurrentQueue<Job>[] queues;
    private readonly uint[] queueGaps;

    public JobsInvokerWorker Invoker { get; set; }
    public bool IsDisposed { get; private set; }

    public long CurrentRawTime => JobsWorld.CurrentRawTimeWorker(startRealTime);

    public JobsQueue(JobsInvoker invoker, uint[]? queues, long startRealTime) {
        Invoker = invoker.Worker;
        this.startRealTime = startRealTime;

        // Default queues
        queues ??= new uint[] {
            5_000, // 5 seconds
            30_000, // 30 seconds
            120_000, // 2 minutes
            300_000, // 5 minutes
            900_000, // 15 minutes
            3600_000, // 1 hour
        };
        queueGaps = queues[0] != 0 ? queues.Prepend(0u).ToArray() : queues;

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
    }

    public void Enqueue(Job job) {
        long time = job.RawTime - CurrentRawTime;
        for (int i = queueGaps.Length - 1; i >= 0; i--) {
            if (time >= queueGaps[i]) {
                queues[i].Enqueue(job);
                return;
            }
        }

        Debug.Assert(time <= 0);
        Invoker.EnqueueJobToInvoke(job);
    }

    private void QueueSortThreadWork(object? indexObj) {
        int index = (int)indexObj!;
        ConcurrentQueue<Job> queue = queues[index];

        int waitTime = (int)Math.Max(queueGaps[index] - 1, 1);
        long jumpTime = index > 1 ? queueGaps[index - 1] : 0;
        long rawTime;
        long timeToExecute;
        Job? first = null;

        while (!IsDisposed) {
            rawTime = CurrentRawTime;
            while (queue.TryDequeue(out Job? job)) {
                if (job == first) {
                    if (!job.IsUsed)
                        queue.Enqueue(job);
                    break;
                }

                if (job.IsUsed)
                    continue;

                timeToExecute = job.RawTime - rawTime;
                if (timeToExecute > jumpTime) {
                    first ??= job;
                    queue.Enqueue(job);
                    continue;
                }

                bool breaked = false;
                for (int i = index - 1; i <= 0; i--) {
                    if (queueGaps[i] > timeToExecute) {
                        queues[i].Enqueue(job);
                        breaked = true;
                        break;
                    }
                }

                if (!breaked) {
                    Debug.Assert(timeToExecute <= 0);
                    Invoker.EnqueueJobToInvoke(job);
                }
            }

            first = null;
            Thread.Sleep(waitTime);
        }
    }

}
