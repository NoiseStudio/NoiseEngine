using Xunit;
using System.Threading;
using NoiseEngine.Jobs;

namespace NoiseEngine.Tests.Jobs;

public class EntityScheduleTest {

    [Fact]
    public void Test1() {
        EntitySchedule schedule = new EntitySchedule();
        EntityWorld world = new EntityWorld();

        TestSystemScheduleA system = new TestSystemScheduleA();
        system.Initialize(world, schedule, 100);

        int entities = 1024;
        for (int i = 0; i < entities; i++)
            world.NewEntity(new TestComponentA(), new TestComponentB());
        while (system.UpdateEntityCount < entities)
            continue;

        while (system.LateUpdateCount < 2)
            continue;

        Assert.True(system.UpdateCount >= 2);
        Assert.True(system.UsedUpdate);
        Assert.True(system.UsedLateUpdate);

        for (int i = 0; i < entities; i++)
            world.NewEntity(new TestComponentA(), new TestComponentB());
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
