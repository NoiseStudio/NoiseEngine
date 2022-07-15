using Xunit;
using System.Threading;
using NoiseEngine.Jobs;

namespace NoiseEngine.Tests.Jobs;

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
