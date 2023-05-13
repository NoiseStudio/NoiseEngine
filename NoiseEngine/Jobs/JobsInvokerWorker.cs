using System;
using System.Collections.Concurrent;
using System.Threading;

namespace NoiseEngine.Jobs2;

internal sealed class JobsInvokerWorker : IDisposable {

    private readonly ConcurrentQueue<Job> jobsToInvoke = new ConcurrentQueue<Job>();
    private readonly ManualResetEventSlim executorThreadsResetEvent = new ManualResetEventSlim(false);

    private bool work = true;
    private int wakeUpExecutorThreadCount;
    private int activeExecutorThreadCount;

    public int ThreadCount { get; }

    public JobsInvokerWorker(int? threadCount) {
        if (threadCount <= 0) {
            throw new ArgumentOutOfRangeException(
                nameof(threadCount), "The number of threads cannot be zero or negative."
            );
        }

        ThreadCount = threadCount ?? Environment.ProcessorCount;
        activeExecutorThreadCount = ThreadCount;

        for (int i = 0; i < ThreadCount; i++) {
            new Thread(ExecutorThreadWork) {
                Name = $"{nameof(JobsInvoker)} worker #{i}",
                IsBackground = true
            }.Start();
        }
    }

    public void Dispose() {
        work = false;
    }

    public void EnqueueJobToInvoke(Job job) {
        jobsToInvoke.Enqueue(job);
        Interlocked.Exchange(ref wakeUpExecutorThreadCount, ThreadCount);
        executorThreadsResetEvent.Set();
    }

    private void ExecutorThreadWork() {
        ManualResetEventSlim executorThreadsResetEvent = this.executorThreadsResetEvent;
        ConcurrentQueue<Job> jobsToInvoke = this.jobsToInvoke;

        try {
            while (work) {
                while (jobsToInvoke.TryDequeue(out Job? job))
                    job.TryInvoke();

                executorThreadsResetEvent.Wait();
                if (Interlocked.Decrement(ref wakeUpExecutorThreadCount) == 0)
                    executorThreadsResetEvent.Reset();
            }
        } catch (Exception exception) {
            Log.Error($"Thread terminated due to an exception. {exception}");
            throw;
        } finally {
            if (Interlocked.Decrement(ref activeExecutorThreadCount) == 0)
                executorThreadsResetEvent.Dispose();
        }
    }

}
