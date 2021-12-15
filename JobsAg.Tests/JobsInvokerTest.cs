using Xunit;
using System.Threading;

namespace NoiseStudio.JobsAg.Tests {
    public class JobsInvokerTest {

        private readonly AutoResetEvent autoResetEvent = new AutoResetEvent(false);

        [Fact]
        public void InvokeJob() {
            JobsInvoker invoker = new JobsInvoker();
            JobsWorld world = new JobsWorld(invoker);

            invoker.InvokeJob(new Job(0, TestMethod, JobTime.Zero), world);

            autoResetEvent.WaitOne();
        }

        private void TestMethod() {
            autoResetEvent.Set();
        }

    }
}
