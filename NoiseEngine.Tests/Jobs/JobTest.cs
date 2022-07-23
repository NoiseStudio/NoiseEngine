using NoiseEngine.Jobs;

namespace NoiseEngine.Tests.Jobs;

public class JobTest {

    [Fact]
    public void Destroy() {
        JobsWorld world = new JobsWorld(new JobsInvoker());
        Job job = world.EnqueueJob(TestMethod, 5);
        job.Destroy(world);
    }

    [Fact]
    public void IsInvoked() {
        JobsWorld world = new JobsWorld(new JobsInvoker());
        Job job = world.EnqueueJob(TestMethod, 0);
        Assert.True(job.IsInvoked(world));

        job = world.EnqueueJob(TestMethod, 5);
        Assert.False(job.IsInvoked(world));
    }

    [Fact]
    public void GetHashCodeTest() {
        Job a = new Job(11, GetHashCodeTest, new JobTime());
        Job b = new Job(11, GetHashCodeTest, new JobTime());
        Job c = new Job(69, GetHashCodeTest, new JobTime());

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
        Assert.NotEqual(a.GetHashCode(), c.GetHashCode());
    }

    [Fact]
    public void EqualsTest() {
        Job a = new Job(420, EqualsTest, new JobTime());
        Job b = new Job(420, EqualsTest, new JobTime());
        Job c = new Job(2137, EqualsTest, new JobTime());

        Assert.True(a.Equals((object)b));
        Assert.False(a.Equals((object)c));
        Assert.False(b.Equals(null));
        Assert.False(c.Equals((ulong)2137));

        Assert.True(a == b);
        Assert.False(a == c);
        Assert.True(a != c);
    }

    [Fact]
    public void EqualsGenericTest() {
        Job a = new Job(36, EqualsGenericTest, new JobTime());
        Job b = new Job(36, EqualsGenericTest, new JobTime());
        Job c = new Job(773, EqualsGenericTest, new JobTime());

        Assert.True(a.Equals(b));
        Assert.False(a.Equals(c));
    }

    private void TestMethod() {
    }

}
