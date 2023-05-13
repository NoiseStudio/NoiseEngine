using System.Threading;

namespace NoiseEngine.Tests.Jobs;

[Collection(nameof(JobsCollectionOld))]
public class JobsWorldTest {

    private readonly AutoResetEvent autoResetEvent = new AutoResetEvent(false);

    private int invokeCount;
    private bool equal;

    private JobsFixture Fixture { get; }

    public JobsWorldTest(JobsFixture fixture) {
        Fixture = fixture;
    }

    [Fact]
    public void TestOneJob() {
        Fixture.JobsWorldFast.EnqueueJob(TestMethod, 15);
        autoResetEvent.WaitOne();
    }

    [Fact]
    public void TestThousandJobs() {
        for (int i = 0; i < 1000; i++)
            Fixture.JobsWorldFast.EnqueueJob(TestMethodThousand, 15);

        autoResetEvent.WaitOne();
    }

    [Fact]
    public void CreateWithoutQueues() {
        Fixture.JobsWorld.EnqueueJob(TestMethodT0, 0);
    }

    [Fact]
    public void CreateWithQueues() {
        Fixture.JobsWorldFast.EnqueueJob(TestMethodT0, 10);
    }

    [Fact]
    public void EnqueueJobT0() {
        Fixture.JobsWorld.EnqueueJob(TestMethodT0, 0);
    }

    [Fact]
    public void EnqueueJobT1() {
        Fixture.JobsWorld.EnqueueJob(TestMethodT1, 0, "Hello");
        autoResetEvent.WaitOne();
        Assert.True(equal);
    }

    private void TestMethodT0() {
    }

    private void TestMethodT1(string a) {
        equal = a == "Hello";
        autoResetEvent.Set();
    }

    private void TestMethod() {
        autoResetEvent.Set();
    }

    private void TestMethodThousand() {
        if (Interlocked.Increment(ref invokeCount) == 1000)
            autoResetEvent.Set();
    }

}
