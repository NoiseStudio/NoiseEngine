using NoiseEngine.Common;
using System;

namespace NoiseEngine.Jobs {
    public class JobsInvoker : Destroyable {

        private readonly JobsInvokerWorker worker;

        /// <summary>
        /// Creates new <see cref="JobsInvoker"/>
        /// </summary>
        /// <param name="threadCount">Number of used threads. When null the number of threads contained in the processor is used.</param>
        /// <exception cref="ArgumentOutOfRangeException">Error when using zero or negative threads</exception>
        public JobsInvoker(int? threadCount = null) {
            worker = new JobsInvokerWorker(threadCount);
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

        protected override void ReleaseResources() {
            worker.Dispose();
        }

    }
}
