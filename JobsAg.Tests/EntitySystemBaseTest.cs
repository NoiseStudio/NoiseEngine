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

        [Fact]
        public void Dependency() {
            EntitySchedule schedule = new EntitySchedule();
            EntityWorld world = new EntityWorld();

            world.NewEntity(new TestComponentA(), new TestComponentB());
            world.NewEntity(new TestComponentA(), new TestComponentB());
            world.NewEntity(new TestComponentA(), new TestComponentB());

            TestSystemA systemA = new TestSystemA();
            TestSystemB systemB = new TestSystemB();
            world.AddSystem(systemA);
            world.AddSystem(systemB);

            systemB.Execute();
            systemB.AddDependency(systemA);

            systemB.ExecuteMultithread();
            Assert.Throws<InvalidOperationException>(systemB.ExecuteMultithread);

            systemA.Execute();
            systemA.Execute();

            systemB.Execute();
            Assert.Throws<InvalidOperationException>(systemB.Execute);
        }

        [Fact]
        public void CanBeExecuted() {
            EntityWorld world = new EntityWorld();

            world.NewEntity(new TestComponentA(), new TestComponentB());
            world.NewEntity(new TestComponentA(), new TestComponentB());
            world.NewEntity(new TestComponentA(), new TestComponentB());

            TestSystemA systemA = new TestSystemA();
            TestSystemB systemB = new TestSystemB();
            world.AddSystem(systemA);
            world.AddSystem(systemB);

            systemB.Execute();
            systemB.AddDependency(systemA);

            Assert.True(systemB.CanExecute);
            systemB.Execute();
            Assert.False(systemB.CanExecute);

            Assert.True(systemA.CanExecute);
        }

    }
}
