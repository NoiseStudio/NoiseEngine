using NoiseEngine.Jobs;
using Xunit;

namespace NoiseEngine.Tests.Jobs;

public class EntitySystemT2Test {

    [Fact]
    public void Execute() {
        EntityWorld world = new EntityWorld();

        world.NewEntity();
        world.NewEntity(new TestComponentA(), new TestComponentB());
        Entity entity = world.NewEntity(new TestComponentA(), new TestComponentB());
        world.NewEntity();
        world.NewEntity(new TestComponentA(), new TestComponentB());

        TestSystemC system = new TestSystemC();
        system.Initialize(world);

        Assert.Equal(0, entity.Get<TestComponentB>(world).A);

        system.TryExecuteAndWait();
        Assert.Equal(1, entity.Get<TestComponentA>(world).A);
        Assert.Equal(4, entity.Get<TestComponentB>(world).A);

        system.Enabled = false;
        system.Enabled = true;

        system.TryExecuteAndWait();
        Assert.Equal(105, entity.Get<TestComponentA>(world).A);
        Assert.Equal(108, entity.Get<TestComponentB>(world).A);
    }

}
