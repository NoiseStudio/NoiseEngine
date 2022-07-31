using System;
using System.Threading;

namespace NoiseEngine.Jobs;

public class JobsWorld : IDisposable {

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

    public delegate void JobT0();
    public delegate void JobT1<T>(T argument0);

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

    /// <summary>
    /// Creates new <see cref="Job"/> in this <see cref="JobsWorld"/>
    /// </summary>
    /// <param name="toExecute">The method that will be executed</param>
    /// <param name="relativeExecutionTime">Relative waiting time in milliseconds to <see cref="Job"/> execution</param>
    /// <returns><see cref="Job"/></returns>
    public Job EnqueueJob(JobT0 toExecute, uint relativeExecutionTime) {
        Job job = EnqueueJobWorker(toExecute, relativeExecutionTime);
        AddNewJobToQueue(job);
        return job;
    }

    /// <summary>
    /// Creates new <see cref="Job"/> in this <see cref="JobsWorld"/>
    /// </summary>
    /// <typeparam name="T">First argument type</typeparam>
    /// <param name="toExecute">The method that will be executed</param>
    /// <param name="relativeExecutionTime">Relative waiting time in milliseconds to <see cref="Job"/> execution</param>
    /// <param name="argument0">First argument</param>
    /// <returns><see cref="Job"/></returns>
    public Job EnqueueJob<T>(JobT1<T> toExecute, uint relativeExecutionTime, T argument0) {
        Job job = EnqueueJobWorker(toExecute, relativeExecutionTime);

        ComponentsStorage.AddComponent(job, argument0);

        AddNewJobToQueue(job);
        return job;
    }

    private Job EnqueueJobWorker(Delegate toExecute, uint relativeExecutionTime) {
        JobTime jobTime = new JobTime((ulong)WorldTime.Time + relativeExecutionTime);
        return new Job(Interlocked.Increment(ref nextJobId), toExecute, jobTime);
    }

    private void AddNewJobToQueue(Job job) {
        queue.Enqueue(job);
    }

}
