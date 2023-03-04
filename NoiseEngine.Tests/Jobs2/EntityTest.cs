using NoiseEngine.Jobs2;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;

namespace NoiseEngine.Tests.Jobs2;

public class EntityTest : ApplicationTestEnvironment {

    public EntityTest(ApplicationFixture fixture) : base(fixture) {
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

}
