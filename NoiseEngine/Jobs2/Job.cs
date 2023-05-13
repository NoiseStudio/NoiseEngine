using System;
using System.Threading;

namespace NoiseEngine.Jobs2;

public abstract class Job : IDisposable {

    private int state;

    public abstract Delegate? Delegate { get; }

    public JobsWorld World { get; }
    public long RawTime { get; }
    public bool IsDisposed => state == 1;
    public bool IsInvoked => state == 2;

    internal bool IsUsed => state != 0;

    private protected Job(JobsWorld world, long rawTime) {
        World = world;
        RawTime = rawTime;
    }

    /// <summary>
    /// Tries invokes this <see cref="Job"/>.
    /// </summary>
    /// <returns>
    /// Returns <see langword="true"/> when <see cref="Job"/> was invoked by this call; otherwise
    /// <see langword="false"/>.
    /// </returns>
    public bool TryInvoke() {
        if (Interlocked.CompareExchange(ref state, 2, 0) != 0)
            return false;

        InvokeWorker();
        DisposeWorker();
        return true;
    }

    /// <summary>
    /// Disposes this <see cref="Job"/>.
    /// </summary>
    public void Dispose() {
        if (Interlocked.CompareExchange(ref state, 1, 0) == 0)
            DisposeWorker();
    }

    private protected abstract void InvokeWorker();

    private protected abstract void DisposeWorker();

}
