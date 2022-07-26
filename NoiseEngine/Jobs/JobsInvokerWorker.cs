using NoiseEngine.Collections.Concurrent;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace NoiseEngine.Jobs;

public class JobsInvokerWorker : IDisposable {

    private readonly AutoResetEvent toInvokeAutoResetEvent = new AutoResetEvent(false);
    private readonly IReadOnlyList<AutoResetEvent> invokeAutoResetEvents;
    private readonly ConcurrentList<JobsQueue> queues = new ConcurrentList<JobsQueue>();
    private readonly ConcurrentQueue<(Job, JobsWorld)> toInvoke = new ConcurrentQueue<(Job, JobsWorld)>();
    private readonly int threadCount;

    private long waitTime;
    private int toInvokeAutoResetEventRelease;
    private int nextInvokeThreadToSignal;

    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Creates new <see cref="JobsInvokerWorker"/>
    /// </summary>
    /// <param name="threadCount">Number of used threads. When null the number of threads contained in the processor is used.</param>
    /// <exception cref="ArgumentOutOfRangeException">Error when using zero or negative threads</exception>
    public JobsInvokerWorker(int? threadCount = null) {
        if (threadCount == null)
            threadCount = Environment.ProcessorCount;

        if (threadCount <= 0)
            throw new ArgumentOutOfRangeException("The number of threads cannot be zero or negative.");

        this.threadCount = (int)threadCount;

        Thread thread = new Thread(ToInvokeThreadWork);
        thread.Name = $"{nameof(JobsInvoker)} end queue";
        thread.Start();

        AutoResetEvent[] invokeAutoResetEvents = new AutoResetEvent[(int)threadCount];
        this.invokeAutoResetEvents = invokeAutoResetEvents;

        for (int i = 0; i < threadCount; i++) {
            invokeAutoResetEvents[i] = new AutoResetEvent(false);

            thread = new Thread(InvokeThreadWork);
            thread.Name = $"{nameof(JobsInvoker)} worker #{i}";
            thread.Start(i);
        }
    }

    /// <summary>
    /// This <see cref="JobsInvokerWorker"/> will be deactivated and disposed
    /// </summary>
    public void Dispose() {
        IsDisposed = true;
    }

    internal void InvokeJob(Job job, JobsWorld world) {
        toInvoke.Enqueue((job, world));
        SignalInvokeThread();
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
        while (!IsDisposed) {
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

        toInvokeAutoResetEvent.Dispose();
    }

    private void InvokeThreadWork(object? threadIdObject) {
        int threadId = (int)threadIdObject!;

        AutoResetEvent autoResetEvent = invokeAutoResetEvents[threadId];

        while (!IsDisposed) {
            autoResetEvent.WaitOne();

            while (toInvoke.TryDequeue(out (Job job, JobsWorld world) jobObject)) {
                jobObject.job.Invoke(jobObject.world);
            }
        }

        autoResetEvent.Dispose();
    }

    private void SignalInvokeThread() {
        invokeAutoResetEvents[Interlocked.Increment(ref nextInvokeThreadToSignal) % threadCount].Set();
    }

}
