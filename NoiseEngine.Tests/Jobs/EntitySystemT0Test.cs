using NoiseEngine.Jobs;

namespace NoiseEngine.Tests.Jobs;

[Collection(nameof(JobsCollection))]
public class EntitySystemT0Test {

    private JobsFixture Fixture { get; }

    public EntitySystemT0Test(JobsFixture fixture) {
        Fixture = fixture;
    }

    [Fact]
    public void Execute() {
        using EntityWorld world = Fixture.CreateEntityWorld();

        world.NewEntity();
        world.NewEntity(new TestComponentA());
        world.NewEntity(new TestComponentA());

        using TestSystemA system = new TestSystemA();
        system.Initialize(world);

        Assert.Equal(-5, system.C);

        system.TryExecuteAndWait();
        Assert.Equal(3, system.C);

        system.Enabled = false;
        system.Enabled = true;

        system.TryExecuteAndWait();
        Assert.Equal(107, system.C);
    }

}
