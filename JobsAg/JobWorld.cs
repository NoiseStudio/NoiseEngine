using System;
using System.Threading;

namespace NoiseStudio.JobsAg {
    public class JobWorld {

        private ulong nextJobId = 1;

        public delegate void JobT0();
        public delegate void JobT1<T>(T argument0);

        public Job NewJob(JobT0 toExecute, ulong relativeExecutionTime) {
            return NewJobWorker(toExecute, relativeExecutionTime);
        }

        public Job NewJob<T>(JobT1<T> toExecute, ulong relativeExecutionTime, T argument0) {
            return NewJobWorker(toExecute, relativeExecutionTime);
        }

        private Job NewJobWorker(Delegate toExecute, ulong relativeExecutionTime) {
            return new Job(Interlocked.Increment(ref nextJobId), toExecute);
        }

    }
}
