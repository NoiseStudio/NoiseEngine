using NoiseEngine.Jobs2;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;

namespace NoiseEngine.Tests.Jobs2;

public class EntityObserverTest : ApplicationTestEnvironment {

    private bool invoked;

    public EntityObserverTest(ApplicationFixture fixture) : base(fixture) {
    }

    [Fact]
    public void Changed() {
        EntityWorld.AddObserver<MockComponentE, MockComponentB>(OnChange);
        using Entity entity = EntityWorld.Spawn(new MockComponentE(5), MockComponentB.TestValueA);

        using TestSystemB system = new TestSystemB();
        EntityWorld.AddSystem(system);
        system.ExecuteAndWait();

        Assert.True(invoked);
    }

    private void OnChange(
        Entity entity, SystemCommands commands, Changed<MockComponentE> componentA, MockComponentB componentB
    ) {
        Assert.Equal(5, componentA.Old.Value);
        Assert.Equal(10, componentA.Current.Value);
        Assert.Equal(MockComponentB.TestValueA, componentB);
        invoked = true;
    }

}
