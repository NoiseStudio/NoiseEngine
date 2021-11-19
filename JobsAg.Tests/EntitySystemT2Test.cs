using Xunit;

namespace NoiseStudio.JobsAg.Tests {
    public class EntitySystemT2Test {

        [Fact]
        public void Test() {
            EntityWorld world = new EntityWorld();

            world.NewEntity();
            world.NewEntity(new TestComponentA(), new TestComponentB());
            Entity entity = world.NewEntity(new TestComponentA(), new TestComponentB());
            world.NewEntity();
            world.NewEntity(new TestComponentA(), new TestComponentB());

            TestSystemC system = new TestSystemC();
            world.AddSystem(system);

            Assert.Equal(0, entity.Get<TestComponentB>(world).A);

            system.Execute();
            Assert.Equal(1, entity.Get<TestComponentA>(world).A);
            Assert.Equal(4, entity.Get<TestComponentB>(world).A);

            world.DisableSystem<TestSystemC>();
            world.EnableSystem<TestSystemC>();

            system.Execute();
            Assert.Equal(105, entity.Get<TestComponentA>(world).A);
            Assert.Equal(108, entity.Get<TestComponentB>(world).A);
        }

    }
}
