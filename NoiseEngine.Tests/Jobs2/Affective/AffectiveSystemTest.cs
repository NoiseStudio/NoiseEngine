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

        // Low.
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

        Assert.True(entityA.TryGet(out MockComponentD d));
        Assert.Equal(1, d.Value);

        // Medium.
        MockAffectiveComponentA medium = new MockAffectiveComponentA(MockAffectivePrecision.Medium);
        SystemCommands commands = new SystemCommands();
        commands.GetEntity(entityB).Insert(medium);
        EntityWorld.ExecuteCommands(commands);

        i = 0;
        foreach (EntitySystem system in affectiveSystem.Systems)
            ((MockAffectiveSystemChild)system).Value = ++i;

        foreach (EntitySystem system in affectiveSystem.Systems)
            system.ExecuteAndWait();

        Assert.True(entityB.TryGet(out d));
        Assert.Equal(2, d.Value);

        // High.
        MockAffectiveComponentA high = new MockAffectiveComponentA(MockAffectivePrecision.High);
        commands = new SystemCommands();
        commands.GetEntity(entityA).Insert(high);
        commands.GetEntity(entityD).Insert(high);
        EntityWorld.ExecuteCommands(commands);

        i = 0;
        foreach (EntitySystem system in affectiveSystem.Systems)
            ((MockAffectiveSystemChild)system).Value = ++i;

        foreach (EntitySystem system in affectiveSystem.Systems)
            system.ExecuteAndWait();

        Assert.True(entityA.TryGet(out d));
        Assert.Equal(3, d.Value);
        Assert.True(entityD.TryGet(out d));
        Assert.Equal(3, d.Value);

        // Execute only two.
        i = 30;
        foreach (EntitySystem system in affectiveSystem.Systems)
            ((MockAffectiveSystemChild)system).Value = ++i;

        i = 0;
        foreach (EntitySystem system in affectiveSystem.Systems) {
            if (i++ % 2 == 0)
                system.ExecuteAndWait();
        }

        Assert.True(entityA.TryGet(out d));
        Assert.Equal(33, d.Value);
        Assert.True(entityB.TryGet(out d));
        Assert.Equal(2, d.Value);
        Assert.True(entityC.TryGet(out d));
        Assert.Equal(31, d.Value);
        Assert.True(entityD.TryGet(out d));
        Assert.Equal(33, d.Value);
    }

}
