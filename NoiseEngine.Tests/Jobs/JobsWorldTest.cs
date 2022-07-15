using NoiseEngine.Jobs;
using System.Threading;
using Xunit;

namespace NoiseEngine.Tests.Jobs;

public class JobsWorldTest {

    private readonly AutoResetEvent autoResetEvent = new AutoResetEvent(false);

    private int invokeCount = 0;
    private bool equal = false;

    [Fact]
    public void TestOneJob() {
        JobsInvoker invoker = new JobsInvoker();
        JobsWorld world = new JobsWorld(invoker, new uint[] {
            2, 3, 5, 10
        });
        world.EnqueueJob(TestMethod, 15);

        autoResetEvent.WaitOne();
    }

    [Fact]
    public void TestThousandJobs() {
        JobsInvoker invoker = new JobsInvoker();
        JobsWorld world = new JobsWorld(invoker, new uint[] {
            2, 3, 5, 10
        });

        for (int i = 0; i < 1000; i++)
            world.EnqueueJob(TestMethodThousand, 15);

        autoResetEvent.WaitOne();
    }

    [Fact]
    public void CreateWithoutQueues() {
        JobsInvoker invoker = new JobsInvoker();
        JobsWorld world = new JobsWorld(invoker, new uint[0]);
        world.EnqueueJob(TestMethodT0, 0);
    }

    [Fact]
    public void CreateWithQueues() {
        JobsInvoker invoker = new JobsInvoker();
        JobsWorld world = new JobsWorld(invoker, new uint[] { 1 });
        world.EnqueueJob(TestMethodT0, 10);
    }

    [Fact]
    public void EnqueueJobT0() {
        JobsInvoker invoker = new JobsInvoker();
        JobsWorld world = new JobsWorld(invoker);
        world.EnqueueJob(TestMethodT0, 0);
    }

    [Fact]
    public void EnqueueJobT1() {
        JobsInvoker invoker = new JobsInvoker();
        JobsWorld world = new JobsWorld(invoker);
        world.EnqueueJob(TestMethodT1, 0, "Hello");

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
