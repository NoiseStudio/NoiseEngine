using System;
using System.Threading;

namespace NoiseStudio.JobsAg {
    public class JobWorld {

        internal readonly JobsQueue queue;

        private readonly JobTime startTime;
        private readonly double startRealTime;

        private ulong nextJobId = 1;

        public JobTime WorldTime {
            get {
                ulong currentSessionTime = (ulong)(Time.UtcMilliseconds - startRealTime);
                return new JobTime(startTime.Time + currentSessionTime);
            }
        }

        internal ComponentsStorage<Job> ComponentsStorage { get; } = new ComponentsStorage<Job>();

        public delegate void JobT0();
        public delegate void JobT1<T>(T argument0);

        public JobWorld(uint[]? queues = null, JobTime ? startTime = null) {
            if (startTime == null)
                startTime = JobTime.Zero;
            this.startTime = (JobTime)startTime;

            startRealTime = Time.UtcMilliseconds;
            queue = new JobsQueue(this, queues);
        }

        /// <summary>
        /// Creates new job in this job world
        /// </summary>
        /// <param name="toExecute">The method that will be executed</param>
        /// <param name="relativeExecutionTime">Relative waiting time in milliseconds to <see cref="Job"/> execution</param>
        /// <returns><see cref="Job"/></returns>
        public Job NewJob(JobT0 toExecute, uint relativeExecutionTime) {
            Job job = NewJobWorker(toExecute, relativeExecutionTime);
            AddNewJobToQueue(job);
            return job;
        }

        /// <summary>
        /// Creates new job in this job world
        /// </summary>
        /// <typeparam name="T">First argument type</typeparam>
        /// <param name="toExecute">The method that will be executed</param>
        /// <param name="relativeExecutionTime">Relative waiting time in milliseconds to <see cref="Job"/> execution</param>
        /// <param name="argument0">First argument</param>
        /// <returns><see cref="Job"/></returns>
        public Job NewJob<T>(JobT1<T> toExecute, uint relativeExecutionTime, T argument0) {
            Job job = NewJobWorker(toExecute, relativeExecutionTime);

            ComponentsStorage.AddComponent(job, argument0);

            AddNewJobToQueue(job);
            return job;
        }

        private Job NewJobWorker(Delegate toExecute, uint relativeExecutionTime) {
            JobTime jobTime = new JobTime(WorldTime.Time + relativeExecutionTime);
            return new Job(Interlocked.Increment(ref nextJobId), toExecute, jobTime);
        }

        private void AddNewJobToQueue(Job job) {
            queue.Enqueue(job);
        }

    }
}
