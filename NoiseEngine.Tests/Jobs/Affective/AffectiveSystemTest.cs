using NoiseEngine.Jobs;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using System.Linq;

namespace NoiseEngine.Tests.Jobs.Affective;

public class AffectiveSystemTest : ApplicationTestEnvironment {

    public AffectiveSystemTest(ApplicationFixture fixture) : base(fixture) {
    }

    [Fact]
    public void SwapByCommands() {
        using MockAffectiveSystem<MockAffectiveSystemChildA> affectiveSystem =
            new MockAffectiveSystem<MockAffectiveSystemChildA>();
        EntityWorld.AddAffectiveSystem(affectiveSystem);

        // Low.
        MockAffectiveComponentA low = new MockAffectiveComponentA(MockAffectivePrecision.Low);
        Entity entityA = EntityWorld.Spawn(low, new MockComponentD(-1));
        Entity entityB = EntityWorld.Spawn(low, new MockComponentD(-1));
        Entity entityC = EntityWorld.Spawn(low, new MockComponentD(-1));
        Entity entityD = EntityWorld.Spawn(low, new MockComponentD(-1));

        int i = 0;
        foreach (EntitySystem system in affectiveSystem.Systems)
            ((MockAffectiveSystemChildA)system).Value = ++i;

        foreach (EntitySystem system in affectiveSystem.Systems)
            system.ExecuteAndWait();

        Assert.True(entityA.TryGet(out MockComponentD? d));
        Assert.Equal(1, d!.Value);

        // Medium.
        MockAffectiveComponentA medium = new MockAffectiveComponentA(MockAffectivePrecision.Medium);
        SystemCommands commands = new SystemCommands();
        commands.GetEntity(entityB).Insert(medium);
        EntityWorld.ExecuteCommands(commands);

        i = 0;
        foreach (EntitySystem system in affectiveSystem.Systems)
            ((MockAffectiveSystemChildA)system).Value = ++i;

        foreach (EntitySystem system in affectiveSystem.Systems)
            system.ExecuteAndWait();

        Assert.True(entityB.TryGet(out d));
        Assert.Equal(2, d!.Value);

        // High.
        MockAffectiveComponentA high = new MockAffectiveComponentA(MockAffectivePrecision.High);
        commands = new SystemCommands();
        commands.GetEntity(entityA).Insert(high);
        commands.GetEntity(entityD).Insert(high);
        EntityWorld.ExecuteCommands(commands);

        i = 0;
        foreach (EntitySystem system in affectiveSystem.Systems)
            ((MockAffectiveSystemChildA)system).Value = ++i;

        foreach (EntitySystem system in affectiveSystem.Systems)
            system.ExecuteAndWait();

        Assert.True(entityA.TryGet(out d));
        Assert.Equal(3, d!.Value);
        Assert.True(entityD.TryGet(out d));
        Assert.Equal(3, d!.Value);

        // Execute only two.
        i = 30;
        foreach (EntitySystem system in affectiveSystem.Systems)
            ((MockAffectiveSystemChildA)system).Value = ++i;

        i = 0;
        foreach (EntitySystem system in affectiveSystem.Systems) {
            if (i++ % 2 == 0)
                system.ExecuteAndWait();
        }

        Assert.True(entityA.TryGet(out d));
        Assert.Equal(33, d!.Value);
        Assert.True(entityB.TryGet(out d));
        Assert.Equal(2, d!.Value);
        Assert.True(entityC.TryGet(out d));
        Assert.Equal(31, d!.Value);
        Assert.True(entityD.TryGet(out d));
        Assert.Equal(33, d!.Value);
    }

    [Fact]
    public void SwapBySystem() {
        using MockAffectiveSystem<MockAffectiveSystemChildB> affectiveSystem =
            new MockAffectiveSystem<MockAffectiveSystemChildB>();
        EntityWorld.AddAffectiveSystem(affectiveSystem);

        // Initialize.
        MockComponentE m1 = new MockComponentE(-1);
        Entity entityA = EntityWorld.Spawn(MockAffectiveComponentA.Low, m1);
        Entity entityB = EntityWorld.Spawn(MockAffectiveComponentA.Medium, m1);
        Entity entityC = EntityWorld.Spawn(MockAffectiveComponentA.High, m1);

        int i = 0;
        foreach (EntitySystem system in affectiveSystem.Systems)
            ((MockAffectiveSystemChildB)system).Value = ++i;

        affectiveSystem.Systems.First().ExecuteAndWait();

        Assert.True(entityA.TryGet(out MockAffectiveComponentA a));
        Assert.Equal(MockAffectivePrecision.Medium, a.Precision);
        Assert.True(entityA.TryGet(out MockComponentE d));
        Assert.Equal(1, d.Value);

        Assert.True(entityB.TryGet(out a));
        Assert.Equal(MockAffectivePrecision.Medium, a.Precision);
        Assert.True(entityB.TryGet(out d));
        Assert.Equal(-1, d.Value);

        Assert.True(entityC.TryGet(out a));
        Assert.Equal(MockAffectivePrecision.High, a.Precision);
        Assert.True(entityC.TryGet(out d));
        Assert.Equal(-1, d.Value);

        // High to low.
        affectiveSystem.Systems.Skip(2).First().ExecuteAndWait();

        Assert.True(entityC.TryGet(out a));
        Assert.Equal(MockAffectivePrecision.Low, a.Precision);
        Assert.True(entityC.TryGet(out d));
        Assert.Equal(3, d.Value);

        // Medium to low.
        foreach (EntitySystem system in affectiveSystem.Systems.Skip(1))
            system.ExecuteAndWait();

        Assert.True(entityA.TryGet(out a));
        Assert.Equal(MockAffectivePrecision.Low, a.Precision);
        Assert.True(entityA.TryGet(out d));
        Assert.Equal(3, d.Value);

        Assert.True(entityB.TryGet(out a));
        Assert.Equal(MockAffectivePrecision.Low, a.Precision);
        Assert.True(entityB.TryGet(out d));
        Assert.Equal(3, d.Value);

        // Compare.
        Assert.True(entityC.TryGet(out a));
        Assert.Equal(MockAffectivePrecision.Low, a.Precision);
        Assert.True(entityC.TryGet(out d));
        Assert.Equal(3, d.Value);

        // Low to medium.
        affectiveSystem.Systems.First().ExecuteAndWait();

        Assert.True(entityA.TryGet(out a));
        Assert.Equal(MockAffectivePrecision.Medium, a.Precision);
        Assert.True(entityA.TryGet(out d));
        Assert.Equal(1, d.Value);

        Assert.True(entityB.TryGet(out a));
        Assert.Equal(MockAffectivePrecision.Medium, a.Precision);
        Assert.True(entityB.TryGet(out d));
        Assert.Equal(1, d.Value);

        Assert.True(entityC.TryGet(out a));
        Assert.Equal(MockAffectivePrecision.Medium, a.Precision);
        Assert.True(entityC.TryGet(out d));
        Assert.Equal(1, d.Value);
    }

}
