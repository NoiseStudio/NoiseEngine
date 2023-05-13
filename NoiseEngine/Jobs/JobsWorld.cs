using System;

namespace NoiseEngine.Jobs;

public partial class JobsWorld : IDisposable {

    private readonly long startRealTime;

    private JobsQueue? queue;
    private JobsInvoker invoker;

    public JobsInvoker Invoker {
        get => invoker;
        set {
            invoker = value;
            JobsQueue? queue = this.queue;
            if (queue is not null)
                queue.Invoker = value.Worker;
        }
    }

    public long CurrentRawTime => CurrentRawTimeWorker(startRealTime);

    /// <summary>
    /// Creates new <see cref="JobsWorld"/>.
    /// </summary>
    /// <param name="invoker"><see cref="JobsInvoker"/> invoking <see cref="Job"/>s assigned to this world.</param>
    /// <param name="queues">Job queues gaps.</param>
    /// <param name="startRawTime">World time, useful for saving (when null time 0 is used).</param>
    public JobsWorld(JobsInvoker? invoker = null, uint[]? queues = null, long? startRawTime = null) {
        startRealTime = Environment.TickCount64 - (startRawTime ?? 0);

        this.invoker = invoker ?? Application.JobsInvoker;
        queue = new JobsQueue(Invoker, queues, startRealTime);
    }

    ~JobsWorld() {
        ReleaseResources();
    }

    internal static long CurrentRawTimeWorker(long startRealTime) {
        return Environment.TickCount64 - startRealTime;
    }

    /// <summary>
    /// Disposes this <see cref="JobsWorld"/>.
    /// </summary>
    public void Dispose() {
        ReleaseResources();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Enqueues new <see cref="Job"/> in this <see cref="JobsWorld"/>.
    /// </summary>
    /// <param name="method">Method to invoke.</param>
    /// <param name="time">Time to invoke new <see cref="Job"/>.</param>
    /// <returns>
    /// New <see cref="Job"/> in this <see cref="JobsWorld"/> which invokes <paramref name="method"/> after given
    /// <paramref name="time"/>.
    /// </returns>
    public Job Enqueue(Action method, long time) {
        time += CurrentRawTime;
        JobEmpty job = new JobEmpty(this, time, method);
        InitializeJob(job);
        return job;
    }

    private void InitializeJob(Job job) {
        JobsQueue queue = this.queue ?? throw new ObjectDisposedException(nameof(JobsWorld));
        queue.Enqueue(job);
    }

    private void ReleaseResources() {
        queue?.Dispose();
        queue = null;
    }

}
