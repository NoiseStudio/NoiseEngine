using NoiseEngine.Jobs;

namespace NoiseEngine.Tests.Jobs;

[Collection(nameof(JobsCollection))]
public class EntityQueryT2Test {

    private JobsFixture Fixture { get; }

    public EntityQueryT2Test(JobsFixture fixture) {
        Fixture = fixture;
    }

    [Fact]
    public void Foreach() {
        using EntityWorld world = Fixture.CreateEntityWorld();
        EntityQuery<TestComponentA, TestComponentB> query =
            new EntityQuery<TestComponentA, TestComponentB>(world);

        world.NewEntity();
        world.NewEntity(new TestComponentA()).Add(world, new TestComponentB());
        world.NewEntity(new TestComponentB());
        world.NewEntity(new TestComponentA(), new TestComponentB());

        int count = 0;
        foreach ((Entity, TestComponentA, TestComponentB) element in query)
            count++;

        Assert.Equal(2, count);
    }

}
