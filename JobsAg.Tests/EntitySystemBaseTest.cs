using System;
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

            system.Enabled = false;
            system.Enabled = true;

            system.ExecuteMultithread();
            Assert.Equal(105, entity.Get<TestComponentA>(world).A);
        }

        [Fact]
        public void Enable() {
            EntityWorld world = new EntityWorld();

            world.NewEntity();
            world.NewEntity(new TestComponentA());
            world.NewEntity(new TestComponentA());

            TestSystemA system = new TestSystemA();
            world.AddSystem(system);

            system.Execute();

            system.Enabled = false;
            Assert.Throws<InvalidOperationException>(system.Execute);

            system.Enabled = true;
            system.Execute();
        }

    }
}
