using NoiseEngine.Jobs;

namespace NoiseEngine.Tests.Jobs;

[Collection(nameof(JobsCollection))]
public class EntityQueryBaseTest {

    private JobsFixture Fixture { get; }

    public EntityQueryBaseTest(JobsFixture fixture) {
        Fixture = fixture;
    }

    [Fact]
    public void ForeachEntities() {
        using EntityWorld world = Fixture.CreateEntityWorld();
        EntityQuery query = new EntityQuery(world);

        world.NewEntity();
        world.NewEntity(new TestComponentA());
        world.NewEntity(new TestComponentB());
        world.NewEntity(new TestComponentA(), new TestComponentB());

        int count = 0;
        foreach (Entity entity in query.Entities)
            count++;

        Assert.Equal(4, count);
    }

}
