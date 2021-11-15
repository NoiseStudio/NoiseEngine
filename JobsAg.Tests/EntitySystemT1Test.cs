using Xunit;

namespace NoiseStudio.JobsAg.Tests {
    public class EntitySystemT1Test {

        [Fact]
        public void Test() {
            EntityWorld world = new EntityWorld();

            world.NewEntity(new TestComponentA());
            Entity entity = world.NewEntity(new TestComponentA());
            world.NewEntity(new TestComponentA());

            TestSystemB system = new TestSystemB();
            world.AddSystem(system);

            Assert.Equal(0, entity.Get<TestComponentA>(world).A);

            system.Execute();
            Assert.Equal(1, entity.Get<TestComponentA>(world).A);

            world.DisableSystem<TestSystemB>();
            world.EnableSystem<TestSystemB>();

            system.Execute();
            Assert.Equal(105, entity.Get<TestComponentA>(world).A);
        }

    }
}
