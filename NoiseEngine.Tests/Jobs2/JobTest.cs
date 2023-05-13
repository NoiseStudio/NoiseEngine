using NoiseEngine.Jobs2;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;

namespace NoiseEngine.Tests.Jobs2;

public class JobTest : ApplicationTestEnvironment {

    private bool invoked;

    public JobTest(ApplicationFixture fixture) : base(fixture) {
    }

    [Fact]
    public void TryInvoke() {
        Job job = new JobEmpty(JobsWorld, 0, TestMethod);
        Assert.True(job.TryInvoke());
        Assert.True(invoked);
        Assert.True(job.IsInvoked);
    }

    [Fact]
    public void Dispose() {
        Job job = new JobEmpty(JobsWorld, 0, TestMethod);
        job.Dispose();
        Assert.True(job.IsDisposed);
        Assert.False(job.TryInvoke());
    }

    private void TestMethod() {
        invoked = true;
    }

}
