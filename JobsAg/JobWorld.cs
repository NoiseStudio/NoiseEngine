using System;
using System.Threading;

namespace NoiseStudio.JobsAg {
    public class JobWorld {

        private ulong nextJobId = 1;

        internal ComponentsStorage<Job> ComponentsStorage { get; } = new ComponentsStorage<Job>();

        public delegate void JobT0();
        public delegate void JobT1<T>(T argument0);

        /// <summary>
        /// Creates new job in this job world
        /// </summary>
        /// <param name="toExecute">The method that will be executed</param>
        /// <param name="relativeExecutionTime">Relative waiting time in milliseconds to <see cref="Job"/> execution</param>
        /// <returns><see cref="Job"/></returns>
        public Job NewJob(JobT0 toExecute, ulong relativeExecutionTime) {
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
        public Job NewJob<T>(JobT1<T> toExecute, ulong relativeExecutionTime, T argument0) {
            Job job = NewJobWorker(toExecute, relativeExecutionTime);

            ComponentsStorage.AddComponent(job, argument0);

            AddNewJobToQueue(job);
            return job;
        }

        private Job NewJobWorker(Delegate toExecute, ulong relativeExecutionTime) {
            JobTime jobTime = new JobTime();
            return new Job(Interlocked.Increment(ref nextJobId), toExecute, jobTime);
        }

        private void AddNewJobToQueue(Job job) {

        }

    }
}
