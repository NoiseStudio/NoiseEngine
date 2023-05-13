using NoiseEngine.Jobs;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using System.Diagnostics;
using System.Threading.Tasks;

namespace NoiseEngine.Tests.Jobs;

public class EntityQueryTest : ApplicationTestEnvironment {

    public EntityQueryTest(ApplicationFixture fixture) : base(fixture) {
    }

    [Fact]
    public void Test() {
        using Entity entityA = EntityWorld.Spawn(
            MockComponentB.TestValueA, new MockComponentE(5), MockComponentA.TestValueA
        );
        using Entity entityB = EntityWorld.Spawn(MockComponentB.TestValueB, new MockComponentE(7));
        using Entity entityC = EntityWorld.Spawn(MockComponentB.TestValueA, new MockComponentE(8));

        using EntityQuery<MockComponentE, MockComponentB> query =
            EntityWorld.GetQuery<MockComponentE, MockComponentB>();
        foreach ((Entity entity, MockComponentE e, MockComponentB b) in query) {
            switch (e.Value) {
                case 5:
                    Assert.Equal(entityA, entity);
                    Assert.Equal(MockComponentB.TestValueA, b);
                    break;
                case 7:
                    Assert.Equal(entityB, entity);
                    Assert.Equal(MockComponentB.TestValueB, b);
                    break;
                case 8:
                    Assert.Equal(entityC, entity);
                    Assert.Equal(MockComponentB.TestValueA, b);
                    break;
                default:
                    throw new UnreachableException();
            }
        }
    }

    [Fact]
    public void TestParrarel() {
        using Entity entityA = EntityWorld.Spawn(
            MockComponentB.TestValueA, new MockComponentE(5), MockComponentA.TestValueA
        );
        using Entity entityB = EntityWorld.Spawn(MockComponentB.TestValueB, new MockComponentE(7));
        using Entity entityC = EntityWorld.Spawn(MockComponentB.TestValueA, new MockComponentE(8));

        Task task = Task.Run(() => {
            bool add = true;
            for (int i = 0; i < 256; i++) {
                SystemCommands commands = new SystemCommands();

                if (add) {
                    commands.GetEntity(entityA).Insert(MockComponentC.TestValueA);
                    commands.GetEntity(entityB).Insert(MockComponentC.TestValueA);
                    commands.GetEntity(entityC).Insert(MockComponentC.TestValueA);
                } else {
                    commands.GetEntity(entityA).Remove<MockComponentC>();
                    commands.GetEntity(entityB).Remove<MockComponentC>();
                    commands.GetEntity(entityC).Remove<MockComponentC>();
                }

                add = !add;
                EntityWorld.ExecuteCommands(commands);
            }
        });

        using EntityQuery<MockComponentE, MockComponentB> query =
            EntityWorld.GetQuery<MockComponentE, MockComponentB>();

        while (!task.IsCompleted) {
            foreach ((Entity entity, MockComponentE e, MockComponentB b) in query) {
                switch (e.Value) {
                    case 5:
                        Assert.Equal(entityA, entity);
                        Assert.Equal(MockComponentB.TestValueA, b);
                        break;
                    case 7:
                        Assert.Equal(entityB, entity);
                        Assert.Equal(MockComponentB.TestValueB, b);
                        break;
                    case 8:
                        Assert.Equal(entityC, entity);
                        Assert.Equal(MockComponentB.TestValueA, b);
                        break;
                }
            }
        }
    }

}
