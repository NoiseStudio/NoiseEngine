using NoiseEngine.Jobs2;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NoiseEngine.Tests.Jobs2;

public class JobsWorldTest : ApplicationTestEnvironment, IDisposable {

    private const string ValueA = "Hello";
    private const float ValueB = 65.82f;

    private readonly AutoResetEvent resetEvent = new AutoResetEvent(false);

    private int invokeCount;

    public JobsWorldTest(ApplicationFixture fixture) : base(fixture) {
    }

    public void Dispose() {
        resetEvent.Dispose();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100000)]
    public void EnqueueAndInvoke(int count) {
        invokeCount = count;
        Parallel.For(0, count, _ => JobsWorld.Enqueue(TestMethod, 15, this, ValueA, ValueB));

        if (!resetEvent.WaitOne(1000))
            throw new TimeoutException("The Jobs was not invoked.");
    }

    private void TestMethod(JobsWorldTest test, string valueA, float valueB) {
        Assert.Equal(this, test);
        Assert.Equal(ValueA, valueA);
        Assert.Equal(ValueB, valueB);

        if (Interlocked.Decrement(ref invokeCount) == 0)
            resetEvent.Set();
    }

}
