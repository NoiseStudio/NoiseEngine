using System;

namespace NoiseEngine.Jobs;

public class JobsInvoker : IDisposable {

    private readonly JobsInvokerWorker worker;

    public bool IsDisposed => worker.IsDisposed;

    /// <summary>
    /// Creates new <see cref="JobsInvoker"/>.
    /// </summary>
    /// <param name="threadCount">
    /// Number of used threads. When null the number of threads contained in the processor is used.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">Error when using zero or negative threads.</exception>
    public JobsInvoker(int? threadCount = null) {
        worker = new JobsInvokerWorker(threadCount);
    }

    ~JobsInvoker() {
        ReleaseResources();
    }

    /// <summary>
    /// Disposes this <see cref="JobsInvoker"/>.
    /// </summary>
    public void Dispose() {
        ReleaseResources();
        GC.SuppressFinalize(this);
    }

    internal void InvokeJob(Job job, JobsWorld world) {
        worker.InvokeJob(job, world);
    }

    internal void SetToInvokeWaitTime(long waitTime) {
        worker?.SetToInvokeWaitTime(waitTime);
    }

    internal void AddJobsQueue(JobsQueue queue) {
        worker.AddJobsQueue(queue);
    }

    internal void RemoveJobsQueue(JobsQueue queue) {
        worker.RemoveJobsQueue(queue);
    }

    private void ReleaseResources() {
        worker.Dispose();
    }

}
