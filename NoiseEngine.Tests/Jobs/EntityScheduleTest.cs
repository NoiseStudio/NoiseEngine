using System.Threading;

namespace NoiseEngine.Tests.Jobs;

[Collection(nameof(JobsCollectionOld))]
public class EntityScheduleTest {

    private JobsFixture Fixture { get; }

    public EntityScheduleTest(JobsFixture fixture) {
        Fixture = fixture;
    }

    [Fact]
    public void Test1() {
        const int Entities = 1024;

        using TestSystemScheduleA system = new TestSystemScheduleA();
        system.Initialize(Fixture.EntityWorld, Fixture.EntitySchedule, 100);

        for (int i = 0; i < Entities; i++)
            Fixture.EntityWorld.NewEntity(new TestComponentA(), new TestComponentB());
        while (system.LateUpdateCount < 2)
            system.LateUpdateResetEvent.WaitOne();

        Assert.Equal(Entities, system.UpdateEntityCount);
        Assert.True(system.UpdateCount >= 2);
        Assert.True(system.UsedUpdate);
        Assert.True(system.UsedLateUpdate);

        for (int i = 0; i < Entities; i++)
            Fixture.EntityWorld.NewEntity(new TestComponentA(), new TestComponentB());

        while (system.LateUpdateCount < 3)
            system.LateUpdateResetEvent.WaitOne();

        Assert.Equal(Entities * 2, system.UpdateEntityCount);
        Assert.True(system.UpdateCount >= 3);
        Assert.True(system.UsedUpdate);
        Assert.True(system.UsedLateUpdate);

        system.LateUpdateResetEvent.WaitOne();
        Assert.Equal(Entities * 2, system.UpdateEntityCount);
    }

}
