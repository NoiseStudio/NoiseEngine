using NoiseEngine.Jobs2;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;

namespace NoiseEngine.Tests.Jobs2;

public class EntityCommandsTest : JobsTestEnvironment {

    public EntityCommandsTest(JobsFixture fixture) : base(fixture) {
    }

    [Fact]
    public void Insert() {
        Entity entityA = EntityWorld.Spawn(MockComponentA.TestValueA);
        Entity entityB = EntityWorld.Spawn(MockComponentA.TestValueA);

        Assert.True(entityA.TryGet(out MockComponentA componentA));
        Assert.Equal(MockComponentA.TestValueA, componentA);
        Assert.True(entityB.TryGet(out componentA));
        Assert.Equal(MockComponentA.TestValueA, componentA);

        SystemCommands commands = new SystemCommands();
        commands.GetEntity(entityA).Insert(MockComponentB.TestValueA);
        commands.GetEntity(entityB).Insert(MockComponentC.TestValueA);
        EntityWorld.ExecuteCommands(commands);

        Assert.True(entityA.TryGet(out MockComponentB componentB));
        Assert.Equal(MockComponentB.TestValueA, componentB);
        Assert.True(entityB.TryGet(out MockComponentC componentC));
        Assert.Equal(MockComponentC.TestValueA, componentC);
    }

    [Fact]
    public void InsertUpdate() {
        Entity entityA = EntityWorld.Spawn(MockComponentA.TestValueA);
        Entity entityB = EntityWorld.Spawn(MockComponentA.TestValueA);

        Assert.True(entityA.TryGet(out MockComponentA componentA));
        Assert.Equal(MockComponentA.TestValueA, componentA);
        Assert.True(entityB.TryGet(out componentA));
        Assert.Equal(MockComponentA.TestValueA, componentA);

        SystemCommands commands = new SystemCommands();
        commands.GetEntity(entityA).Insert(MockComponentA.TestValueB);
        commands.GetEntity(entityB).Insert(MockComponentA.TestValueB);
        EntityWorld.ExecuteCommands(commands);

        Assert.True(entityA.TryGet(out componentA));
        Assert.Equal(MockComponentA.TestValueB, componentA);
        Assert.True(entityB.TryGet(out componentA));
        Assert.Equal(MockComponentA.TestValueB, componentA);
    }

    [Fact]
    public void Remove() {
        Entity entityA = EntityWorld.Spawn(MockComponentA.TestValueA);
        Entity entityB = EntityWorld.Spawn(MockComponentA.TestValueA);

        SystemCommands commands = new SystemCommands();
        commands.GetEntity(entityA).Remove<MockComponentA>();
        commands.GetEntity(entityB).Remove<MockComponentA>();
        EntityWorld.ExecuteCommands(commands);

        Assert.False(entityA.Contains<MockComponentA>());
        Assert.False(entityB.Contains<MockComponentA>());
    }

}
