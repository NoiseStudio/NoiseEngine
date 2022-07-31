using NoiseEngine.Jobs;

namespace NoiseEngine.Tests.Jobs;

[Collection(nameof(JobsCollection))]
public class EntityQueryT0Test {

    private JobsFixture Fixture { get; }

    public EntityQueryT0Test(JobsFixture fixture) {
        Fixture = fixture;
    }

    [Fact]
    public void Foreach() {
        using EntityWorld world = Fixture.EmptyEntityWorld;
        EntityQuery query = new EntityQuery(world);

        world.NewEntity();
        world.NewEntity(new TestComponentA());
        world.NewEntity(new TestComponentB());
        world.NewEntity(new TestComponentA(), new TestComponentB());

        int count = 0;
        foreach (Entity entity in query)
            count++;

        Assert.Equal(4, count);
    }

}
