using NoiseEngine.Jobs;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;

namespace NoiseEngine.Tests.Jobs;

public class AppendComponentDefaultTest : ApplicationTestEnvironment {

    public AppendComponentDefaultTest(ApplicationFixture fixture) : base(fixture) {
    }

    [Fact]
    public void Test() {
        // Create and check.
        using Entity entityA = EntityWorld.Spawn(MockComponentA.TestValueA, new MockComponentF(7.05m));
        using Entity entityB = EntityWorld.Spawn(new MockComponentF(8.05m), MockComponentB.TestValueB);

        Assert.True(entityA.TryGet(out MockComponentA? a));
        Assert.Equal(MockComponentA.TestValueA, a);
        Assert.True(entityA.TryGet(out MockComponentF f));
        Assert.Equal(7.05m, f.Value);
        Assert.True(entityA.TryGet(out MockComponentB b));
        Assert.Equal(default, b);

        Assert.True(entityB.TryGet(out f));
        Assert.Equal(8.05m, f.Value);
        Assert.True(entityB.TryGet(out b));
        Assert.Equal(MockComponentB.TestValueB, b);

        // Try remove appended component.
        SystemCommands commands = new SystemCommands();
        commands.GetEntity(entityA).Remove<MockComponentB>();
        commands.GetEntity(entityB).Remove<MockComponentB>();
        EntityWorld.ExecuteCommands(commands);

        Assert.True(entityA.TryGet(out a));
        Assert.Equal(MockComponentA.TestValueA, a);
        Assert.True(entityA.TryGet(out f));
        Assert.Equal(7.05m, f.Value);
        Assert.True(entityA.TryGet(out b));
        Assert.Equal(default, b);

        Assert.True(entityB.TryGet(out f));
        Assert.Equal(8.05m, f.Value);
        Assert.True(entityB.TryGet(out b));
        Assert.Equal(MockComponentB.TestValueB, b);

        // Compare archetypes.
        using Entity entityC = EntityWorld.Spawn(
            MockComponentB.TestValueB, MockComponentA.TestValueB, new MockComponentF(9.05m)
        );
        Assert.Equal(entityA.chunk!.Archetype, entityC.chunk!.Archetype);

        // Remove component with appended component default attribute.
        commands = new SystemCommands();
        commands.GetEntity(entityA).Remove<MockComponentB>().Remove<MockComponentF>();
        EntityWorld.ExecuteCommands(commands);

        Assert.True(entityA.TryGet(out a));
        Assert.Equal(MockComponentA.TestValueA, a);
        Assert.True(!entityA.Contains<MockComponentF>());
        Assert.True(!entityA.Contains<MockComponentB>());
    }

}
