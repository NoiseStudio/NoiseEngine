using System;
using System.Collections.Concurrent;
using System.Threading;

namespace NoiseEngine.Jobs2;

internal sealed class JobsQueue : IDisposable {

    private readonly long startRealTime;
    private readonly ConcurrentQueue<Job>[] queues;
    private readonly uint[] queueGaps;
    private readonly AutoResetEvent finalResetEvent = new AutoResetEvent(false);
    private readonly ConcurrentQueue<Job> finalQueue = new ConcurrentQueue<Job>();
    private long finalWaitTime = long.MaxValue;

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
        queueGaps = queues;

        new Thread(FinalQueueThreadWork) {
            Name = $"{nameof(JobsQueue)} final sorting thread",
            Priority = ThreadPriority.BelowNormal,
            IsBackground = true
        }.Start();

        this.queues = new ConcurrentQueue<Job>[queues.Length];
        for (int i = 0; i < this.queues.Length; i++) {
            this.queues[i] = new ConcurrentQueue<Job>();

            new Thread(QueueSortThreadWork) {
                Name = $"{nameof(JobsQueue)} sorting thread #{i}",
                Priority = ThreadPriority.BelowNormal,
                IsBackground = true
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

        EnqueueToFinal(job, time);
    }

    private void QueueSortThreadWork(object? indexObj) {
        int index = (int)indexObj!;
        ConcurrentQueue<Job> queue = queues[index];

        int waitTime = (int)Math.Max((long)queueGaps[index] - 1, 1);
        long jumpTime = index > 1 ? queueGaps[index - 1] : 0;
        long rawTime;
        long timeToExecute;
        Job? first = null;

        try {
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
                    for (int i = index - 1; i >= 0; i--) {
                        if (queueGaps[i] > timeToExecute) {
                            queues[i].Enqueue(job);
                            breaked = true;
                            break;
                        }
                    }

                    if (!breaked)
                        EnqueueToFinal(job, timeToExecute);
                }

                first = null;
                Thread.Sleep(waitTime);
            }
        } catch (Exception exception) {
            Log.Error($"Thread terminated due to an exception. {exception}");
            throw;
        }
    }

    private void FinalQueueThreadWork() {
        AutoResetEvent finalResetEvent = this.finalResetEvent;
        ConcurrentQueue<Job> finalQueue = this.finalQueue;

        long waitTime;
        long rawTime;
        long timeToExecute;
        Job? first = null;

        try {
            while (!IsDisposed) {
                waitTime = long.MaxValue;
                rawTime = CurrentRawTime;
                while (finalQueue.TryDequeue(out Job? job)) {
                    if (job == first) {
                        if (!job.IsUsed)
                            finalQueue.Enqueue(job);
                        break;
                    }

                    if (job.IsUsed)
                        continue;

                    timeToExecute = job.RawTime - rawTime;
                    if (timeToExecute <= 0) {
                        Invoker.EnqueueJobToInvoke(job);
                        continue;
                    }

                    first ??= job;
                    finalQueue.Enqueue(job);

                    if (waitTime > timeToExecute)
                        waitTime = timeToExecute;
                }

                first = null;
                finalWaitTime = waitTime == long.MaxValue ? long.MaxValue : (waitTime + Environment.TickCount);
                finalResetEvent.WaitOne(waitTime == long.MaxValue ? -1 : (int)waitTime);
            }
        } catch (Exception exception) {
            Log.Error($"Thread terminated due to an exception. {exception}");
            throw;
        } finally {
            finalResetEvent.Dispose();
        }
    }

    private void EnqueueToFinal(Job job, long timeToExecute) {
        if (timeToExecute <= 0) {
            Invoker.EnqueueJobToInvoke(job);
            return;
        }

        finalQueue.Enqueue(job);
        if (finalWaitTime > Environment.TickCount64 + timeToExecute)
            finalResetEvent.Set();
    }

}
