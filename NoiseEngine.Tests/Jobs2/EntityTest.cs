using NoiseEngine.Jobs2;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using System.Threading;

namespace NoiseEngine.Tests.Jobs2;

public class EntityTest : JobsTestEnvironment {

    public EntityTest(JobsFixture fixture) : base(fixture) {
    }

    [Fact]
    public void Contains() {
        using Entity entity = EntityWorld.Spawn(MockComponentA.TestValueA);

        Assert.True(entity.Contains<MockComponentA>());
        Assert.False(entity.Contains<MockComponentB>());
    }

    [Fact]
    public void TryGet() {
        using Entity entity = EntityWorld.Spawn(MockComponentA.TestValueA);

        Assert.True(entity.TryGet(out MockComponentA componentA));
        Assert.Equal(MockComponentA.TestValueA, componentA);
    }

    [Fact]
    public void TryAdd() {
        using Entity entity = EntityWorld.Spawn(MockComponentA.TestValueA);

        Assert.True(entity.TryGet(out MockComponentA componentA));
        Assert.Equal(MockComponentA.TestValueA, componentA);

        Assert.False(entity.TryGet(out MockComponentB componentB));

        Assert.True(entity.TryAdd(MockComponentB.TestValueA));
        Assert.True(entity.TryGet(out componentB));
        Assert.Equal(MockComponentB.TestValueA, componentB);

        Assert.True(entity.TryGet(out componentA));
        Assert.Equal(MockComponentA.TestValueA, componentA);
    }

    [Fact]
    public void TryRemove() {
        using Entity entity = EntityWorld.Spawn(MockComponentA.TestValueA);

        Assert.True(entity.TryGet(out MockComponentA componentA));
        Assert.Equal(MockComponentA.TestValueA, componentA);

        Assert.False(entity.TryGet(out MockComponentB componentB));

        Assert.True(entity.TryAdd(MockComponentB.TestValueA));
        Assert.True(entity.TryGet(out componentB));
        Assert.Equal(MockComponentB.TestValueA, componentB);

        Assert.True(entity.TryGet(out componentA));
        Assert.Equal(MockComponentA.TestValueA, componentA);

        Assert.True(entity.TryRemove<MockComponentA>());
        Assert.False(entity.TryGet(out componentA));

        Assert.True(entity.TryGet(out componentB));
        Assert.Equal(MockComponentB.TestValueA, componentB);
    }

    [Fact]
    public void Parallel() {
        RunParallel(TryRemove);

        using Entity entity = EntityWorld.Spawn(MockComponentB.TestValueA);
        int i = 0;

        RunParallel(() => {
            if (Interlocked.Increment(ref i) % 2 == 0) {
                entity.TryAdd(MockComponentA.TestValueA);
                entity.TryGet(out MockComponentA componentA);
                entity.TryRemove<MockComponentA>();
            } else {
                entity.TryRemove<MockComponentC>();
                entity.TryAdd(MockComponentC.TestValueA);
                entity.TryGet(out MockComponentC componentA);
            }

            Assert.True(entity.Contains<MockComponentB>());
        });

        Assert.False(entity.Contains<MockComponentA>());
        Assert.True(entity.Contains<MockComponentB>());
        Assert.True(entity.Contains<MockComponentC>());
    }

}
