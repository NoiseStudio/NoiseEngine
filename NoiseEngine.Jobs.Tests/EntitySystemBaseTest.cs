using System;
using Xunit;

namespace NoiseEngine.Jobs.Tests {
    public class EntitySystemBaseTest {

        [Fact]
        public void ExecuteMultithread() {
            EntitySchedule schedule = new EntitySchedule();
            EntityWorld world = new EntityWorld();

            world.NewEntity(new TestComponentA());
            Entity entity = world.NewEntity(new TestComponentA());
            world.NewEntity(new TestComponentA());

            TestSystemB system = new TestSystemB();
            Assert.Throws<InvalidOperationException>(system.ExecuteMultithread);

            world.AddSystem(system, schedule);

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
            world.AddSystem(systemA, schedule);
            world.AddSystem(systemB, schedule);

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
        public void CanExecute() {
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

        [Fact]
        public void ThreadId() {
            int threadCount = 16;
            EntitySchedule schedule = new EntitySchedule();
            EntityWorld world = new EntityWorld();

            for (int i = 1; i <= threadCount; i++) {
                world.NewEntity(new TestComponentA() {
                    A = 2 * (int)Math.Pow(2, i) + 4
                });
            }

            TestSystemThreadId system = new TestSystemThreadId();
            world.AddSystem(system, schedule);

            system.ExecuteMultithread();
            Assert.Equal(262204 / threadCount, system.AverageTestComponentAAValue);
        }

        [Fact]
        public void Filter() {
            EntityWorld world = new EntityWorld();
            for (int i = 0; i < 16; i++) {
                world.NewEntity();
                world.NewEntity(new TestComponentA());
                world.NewEntity(new TestComponentB());
                world.NewEntity(new TestComponentA(), new TestComponentB());
            }

            TestSystemCounter system = new TestSystemCounter();
            world.AddSystem(system);

            system.Execute();
            Assert.Equal(64, system.EntityCount);

            system.Filter = new EntityFilter(new Type[] {
                typeof(TestComponentA)
            });
            system.Execute();
            Assert.Equal(32, system.EntityCount);

            system.Filter = new EntityFilter(
                new Type[] {
                    typeof(TestComponentA)
                },
                new Type[] {
                    typeof(TestComponentB)
                }
            );
            system.Execute();
            Assert.Equal(16, system.EntityCount);
        }

    }
}
