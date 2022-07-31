namespace NoiseEngine.Jobs;

public partial class JobsWorld : IDisposable {

    internal readonly JobsQueue queue;

    private readonly JobTime startTime;
    private readonly double startRealTime;

    private ulong nextJobId;

    public bool IsDisposed => queue.IsDisposed;

    public JobTime WorldTime {
        get {
            ulong currentSessionTime = (ulong)(Time.UtcMilliseconds - startRealTime);
            return new JobTime((ulong)startTime.Time + currentSessionTime);
        }
    }

    internal ComponentsStorage<Job> ComponentsStorage { get; } = new ComponentsStorage<Job>();

    /// <summary>
    /// Creates new <see cref="JobsWorld"/>
    /// </summary>
    /// <param name="queues">Job queues gaps (affect Jobs performance when default queue is used null)</param>
    /// <param name="invoker"><see cref="JobsInvoker"/> invoking <see cref="Job"/>s assigned to this world</param>
    /// <param name="startTime">World time, useful for saving (when null time 0 is used)</param>
    public JobsWorld(JobsInvoker invoker, uint[]? queues = null, JobTime? startTime = null) {
        if (startTime == null)
            startTime = JobTime.Zero;
        this.startTime = (JobTime)startTime;

        startRealTime = Time.UtcMilliseconds;
        queue = new JobsQueue(this, invoker, queues);
    }

    /// <summary>
    /// Disposes this <see cref="JobsWorld"/>.
    /// </summary>
    public void Dispose() {
        queue.Dispose();
    }

    private Job EnqueueJobWorker(Delegate toExecute, uint relativeExecutionTime) {
        JobTime jobTime = new JobTime((ulong)WorldTime.Time + relativeExecutionTime);
        return new Job(Interlocked.Increment(ref nextJobId), toExecute, jobTime);
    }

    private void AddNewJobToQueue(Job job) {
        queue.Enqueue(job);
    }

}
