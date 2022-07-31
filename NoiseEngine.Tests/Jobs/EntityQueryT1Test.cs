using NoiseEngine.Jobs;

namespace NoiseEngine.Tests.Jobs;

[Collection(nameof(JobsCollection))]
public class EntityQueryT1Test {

    private JobsFixture Fixture { get; }

    public EntityQueryT1Test(JobsFixture fixture) {
        Fixture = fixture;
    }

    [Fact]
    public void Foreach() {
        using EntityWorld world = Fixture.EmptyEntityWorld;
        EntityQuery<TestComponentA> query = new EntityQuery<TestComponentA>(world);

        world.NewEntity();
        world.NewEntity(new TestComponentA());
        world.NewEntity(new TestComponentB());
        world.NewEntity(new TestComponentA(), new TestComponentB());

        int count = 0;
        foreach ((Entity, TestComponentA) element in query)
            count++;

        Assert.Equal(2, count);
    }

}
