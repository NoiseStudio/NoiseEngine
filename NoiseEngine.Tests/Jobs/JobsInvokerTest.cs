using NoiseEngine.Jobs;
using System.Threading;

namespace NoiseEngine.Tests.Jobs;

[Collection(nameof(JobsCollectionOld))]
public class JobsInvokerTest {

    private readonly AutoResetEvent autoResetEvent = new AutoResetEvent(false);

    private JobsFixture Fixture { get; }

    public JobsInvokerTest(JobsFixture fixture) {
        Fixture = fixture;
    }

    [Fact]
    public void InvokeJob() {
        Fixture.JobsInvoker.InvokeJob(new Job(0, TestMethod, JobTime.Zero), Fixture.JobsWorld);
        autoResetEvent.WaitOne();
    }

    private void TestMethod() {
        autoResetEvent.Set();
    }

}
