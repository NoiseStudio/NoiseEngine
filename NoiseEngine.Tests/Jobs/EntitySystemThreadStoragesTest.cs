using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;

namespace NoiseEngine.Tests.Jobs;

public class EntitySystemThreadStoragesTest : ApplicationTestEnvironment {

    public EntitySystemThreadStoragesTest(ApplicationFixture fixture) : base(fixture) {
    }

    [Fact]
    public void Execution() {
        const int Entities = 1024;

        for (int i = 0; i < Entities; i++)
            EntityWorld.Spawn();

        using TestSystemD system = new TestSystemD();
        EntityWorld.AddSystem(system);

        system.ExecuteAndWait();
        Assert.Equal(Entities, system.Count);
    }

}
