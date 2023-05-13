using System;

namespace NoiseEngine.Jobs2;

public class JobsInvoker : IDisposable {

    internal JobsInvokerWorker Worker { get; }

    /// <summary>
    /// Creates new <see cref="JobsInvoker"/>.
    /// </summary>
    /// <param name="threadCount">
    /// Number of used threads. When null the number of threads contained in the processor is used.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">Error when using zero or negative threads.</exception>
    public JobsInvoker(int? threadCount = null) {
        Worker = new JobsInvokerWorker(threadCount);
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

    private void ReleaseResources() {
        Worker.Dispose();
    }

}
