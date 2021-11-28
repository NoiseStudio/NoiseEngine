using Xunit;

namespace NoiseStudio.JobsAg.Tests {
    public class EntitySystemBaseTest {

        [Fact]
        public void ExecuteMultithread() {
            EntitySchedule schedule = new EntitySchedule();
            EntityWorld world = new EntityWorld();

            world.NewEntity(new TestComponentA());
            Entity entity = world.NewEntity(new TestComponentA());
            world.NewEntity(new TestComponentA());

            TestSystemB system = new TestSystemB();
            world.AddSystem(system);

            Assert.Equal(0, entity.Get<TestComponentA>(world).A);

            system.ExecuteMultithread();
            Assert.Equal(1, entity.Get<TestComponentA>(world).A);

            world.DisableSystem<TestSystemB>();
            world.EnableSystem<TestSystemB>();

            system.ExecuteMultithread();
            Assert.Equal(105, entity.Get<TestComponentA>(world).A);
        }

    }
}
