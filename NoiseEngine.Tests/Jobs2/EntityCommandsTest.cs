using NoiseEngine.Jobs2;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NoiseEngine.Tests.Jobs2;

public class EntityCommandsTest : ApplicationTestEnvironment {

    public EntityCommandsTest(ApplicationFixture fixture) : base(fixture) {
    }

    [Fact]
    public void Despawn() {
        Entity entityA = EntityWorld.Spawn(MockComponentA.TestValueA);
        Entity entityB = EntityWorld.Spawn(MockComponentA.TestValueA);

        Assert.True(entityA.TryGet(out MockComponentA componentA));
        Assert.Equal(MockComponentA.TestValueA, componentA);
        Assert.True(entityB.TryGet(out componentA));
        Assert.Equal(MockComponentA.TestValueA, componentA);

        SystemCommands commands = new SystemCommands();
        commands.GetEntity(entityA).Despawn();
        commands.GetEntity(entityB).Despawn();
        EntityWorld.ExecuteCommands(commands);

        Assert.False(entityA.Contains<MockComponentA>());
        Assert.False(entityA.Contains<MockComponentA>());
        Assert.True(entityA.IsDespawned);
        Assert.True(entityB.IsDespawned);
    }

    [Fact]
    public void Insert() {
        using Entity entityA = EntityWorld.Spawn(MockComponentA.TestValueA);
        using Entity entityB = EntityWorld.Spawn(MockComponentA.TestValueA);

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
        using Entity entityA = EntityWorld.Spawn(MockComponentA.TestValueA);
        using Entity entityB = EntityWorld.Spawn(MockComponentA.TestValueA);

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
        using Entity entityA = EntityWorld.Spawn(MockComponentA.TestValueA);
        using Entity entityB = EntityWorld.Spawn(MockComponentA.TestValueA);

        SystemCommands commands = new SystemCommands();
        commands.GetEntity(entityA).Remove<MockComponentA>();
        commands.GetEntity(entityB).Remove<MockComponentA>();
        EntityWorld.ExecuteCommands(commands);

        Assert.False(entityA.Contains<MockComponentA>());
        Assert.False(entityB.Contains<MockComponentA>());
    }

    [Fact]
    public void Parallel() {
        Entity[] entities = Enumerable.Range(0, Environment.ProcessorCount)
            .Select(_ => EntityWorld.Spawn(MockComponentA.TestValueA)).ToArray();

        Task[] tasks = new Task[Environment.ProcessorCount * 4];
        for (int i = 0; i < tasks.Length; i++) {
            tasks[i] = Task.Run(() => {
                for (int i = 0; i < entities.Length * 4; i++) {
                    Entity entity = entities[Random.Shared.Next(entities.Length)];
                    SystemCommands commands = new SystemCommands();

                    if (entity.TryGet(out MockComponentB componentB))
                        Assert.Equal(MockComponentB.TestValueA, componentB);
                    if (entity.TryGet(out MockComponentC componentC))
                        Assert.Equal(MockComponentC.TestValueA, componentC);

                    switch (Random.Shared.Next(5)) {
                        case 0:
                            commands.GetEntity(entity).Insert(MockComponentB.TestValueA);
                            break;
                        case 1:
                            commands.GetEntity(entity).Insert(MockComponentC.TestValueA);
                            break;
                        case 2:
                            commands.GetEntity(entity).Remove<MockComponentB>();
                            break;
                        case 3:
                            commands.GetEntity(entity).Remove<MockComponentC>();
                            break;
                        case 4:
                            commands.GetEntity(entity).Despawn();
                            break;
                    }

                    EntityWorld.ExecuteCommands(commands);
                }
            });
        }

        Task.WaitAll(tasks);

        foreach (Entity entity in entities) {
            if (!entity.IsDespawned) {
                Assert.True(entity.TryGet(out MockComponentA componentA));
                Assert.Equal(MockComponentA.TestValueA, componentA);
                entity.Despawn();
            }
        }
    }

}
