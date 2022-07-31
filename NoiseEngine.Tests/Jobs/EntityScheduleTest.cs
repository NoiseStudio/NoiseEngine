using System.Threading;

namespace NoiseEngine.Tests.Jobs;

[Collection(nameof(JobsCollection))]
public class EntityScheduleTest {

    private JobsFixture Fixture { get; }

    public EntityScheduleTest(JobsFixture fixture) {
        Fixture = fixture;
    }

    [Fact]
    public void Test1() {
        using TestSystemScheduleA system = new TestSystemScheduleA();
        system.Initialize(Fixture.EntityWorld, Fixture.EntitySchedule, 100);

        int entities = 1024;
        for (int i = 0; i < entities; i++)
            Fixture.EntityWorld.NewEntity(new TestComponentA(), new TestComponentB());
        while (system.UpdateEntityCount < entities)
            continue;

        while (system.LateUpdateCount < 2)
            continue;

        Assert.True(system.UpdateCount >= 2);
        Assert.True(system.UsedUpdate);
        Assert.True(system.UsedLateUpdate);

        for (int i = 0; i < entities; i++)
            Fixture.EntityWorld.NewEntity(new TestComponentA(), new TestComponentB());
        while (system.UpdateEntityCount < entities * 2)
            continue;

        while (system.LateUpdateCount < 3)
            continue;

        Assert.True(system.UpdateCount >= 3);
        Assert.True(system.UsedUpdate);
        Assert.True(system.UsedLateUpdate);

        Thread.Sleep(50);
        Assert.True(entities * 2 <= system.UpdateEntityCount);
    }

}
