using NoiseEngine.Jobs;

namespace NoiseEngine.Tests.Jobs;

[Collection(nameof(JobsCollectionOld))]
public class EntitySystemT1Test {

    private JobsFixture Fixture { get; }

    public EntitySystemT1Test(JobsFixture fixture) {
        Fixture = fixture;
    }

    [Fact]
    public void Execute() {
        using EntityWorld world = Fixture.CreateEntityWorld();

        world.NewEntity(new TestComponentA());
        Entity entity = world.NewEntity(new TestComponentA());
        world.NewEntity(new TestComponentA());

        using TestSystemB system = new TestSystemB();
        system.Initialize(world);

        Assert.Equal(0, entity.Get<TestComponentA>(world).A);

        system.TryExecuteAndWait();
        Assert.Equal(1, entity.Get<TestComponentA>(world).A);

        system.Enabled = false;
        system.Enabled = true;

        system.TryExecuteAndWait();
        Assert.Equal(105, entity.Get<TestComponentA>(world).A);
    }

}
