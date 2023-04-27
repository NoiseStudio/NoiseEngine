using NoiseEngine.Jobs2;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;

namespace NoiseEngine.Tests.Jobs2.Affective;

public class AffectiveSystemTest : ApplicationTestEnvironment {

    public AffectiveSystemTest(ApplicationFixture fixture) : base(fixture) {
    }

    [Fact]
    public void Swap() {
        using MockAffectiveSystem affectiveSystem = new MockAffectiveSystem();
        EntityWorld.AddAffectiveSystem(affectiveSystem);

        MockAffectiveComponentA low = new MockAffectiveComponentA(MockAffectivePrecision.Low);
        MockComponentD m1 = new MockComponentD(-1);
        Entity entityA = EntityWorld.Spawn(low, m1);
        Entity entityB = EntityWorld.Spawn(low, m1);
        Entity entityC = EntityWorld.Spawn(low, m1);
        Entity entityD = EntityWorld.Spawn(low, m1);

        int i = 0;
        foreach (EntitySystem system in affectiveSystem.Systems)
            ((MockAffectiveSystemChild)system).Value = ++i;

        foreach (EntitySystem system in affectiveSystem.Systems)
            system.ExecuteAndWait();

        Assert.True(entityA.TryGet(out MockComponentD a));
        Assert.Equal(1, a.Value);
    }

}
