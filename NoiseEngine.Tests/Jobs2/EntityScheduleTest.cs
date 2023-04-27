using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;

namespace NoiseEngine.Tests.Jobs2;

public class EntityScheduleTest : ApplicationTestEnvironment {

    public EntityScheduleTest(ApplicationFixture fixture) : base(fixture) {
    }

    [Fact]
    public void Execution() {
        const int Entities = 1024;

        for (int i = 0; i < Entities; i++)
            EntityWorld.Spawn();

        ScheduleTestSystemA system = new ScheduleTestSystemA();
        EntityWorld.AddSystem(system, 100);

        while (system.LateUpdateCount < 2)
            system.LateUpdateResetEvent.WaitOne();

        Assert.Equal(Entities, system.UpdateEntityCount);
        Assert.True(system.UpdateCount >= 2);
        Assert.True(system.UsedUpdate);
        Assert.True(system.UsedLateUpdate);

        for (int i = 0; i < Entities; i++)
            EntityWorld.Spawn();

        while (system.LateUpdateCount < 3)
            system.LateUpdateResetEvent.WaitOne();

        Assert.Equal(Entities * 2, system.UpdateEntityCount);
        Assert.True(system.UpdateCount >= 3);
        Assert.True(system.UsedUpdate);
        Assert.True(system.UsedLateUpdate);

        system.LateUpdateResetEvent.WaitOne();
        Assert.Equal(Entities * 2, system.UpdateEntityCount);

        system.Dispose();
        Assert.Equal(1, system.DisposeCount);
    }

}
