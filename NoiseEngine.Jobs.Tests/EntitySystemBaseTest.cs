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
            Assert.Throws<InvalidOperationException>(() => system.TryExecuteParallelAndWait());

            system.Initialize(world, schedule);

            Assert.Equal(0, entity.Get<TestComponentA>(world).A);

            system.TryExecuteParallelAndWait();
            Assert.Equal(1, entity.Get<TestComponentA>(world).A);

            system.Enabled = false;
            system.Enabled = true;

            system.TryExecuteParallelAndWait();
            Assert.Equal(105, entity.Get<TestComponentA>(world).A);
        }

        [Fact]
        public void Enable() {
            EntityWorld world = new EntityWorld();

            world.NewEntity();
            world.NewEntity(new TestComponentA());
            world.NewEntity(new TestComponentA());

            TestSystemA system = new TestSystemA();
            system.Initialize(world);

            system.TryExecuteAndWait();

            system.Enabled = false;
            Assert.False(system.TryExecuteAndWait());

            system.Enabled = true;
            system.TryExecuteAndWait();
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
            systemA.Initialize(world, schedule);
            systemB.Initialize(world, schedule);

            systemB.TryExecuteAndWait();
            systemB.AddDependency(systemA);

            systemB.TryExecuteParallelAndWait();
            Assert.False(systemB.TryExecute());

            systemA.TryExecuteAndWait();
            systemA.TryExecuteAndWait();

            systemB.TryExecuteAndWait();
            Assert.False(systemB.TryExecuteAndWait());
        }

        [Fact]
        public void CanExecute() {
            EntityWorld world = new EntityWorld();

            world.NewEntity(new TestComponentA(), new TestComponentB());
            world.NewEntity(new TestComponentA(), new TestComponentB());
            world.NewEntity(new TestComponentA(), new TestComponentB());

            TestSystemA systemA = new TestSystemA();
            TestSystemB systemB = new TestSystemB();
            systemA.Initialize(world);
            systemB.Initialize(world);

            systemB.TryExecuteAndWait();
            systemB.AddDependency(systemA);

            Assert.True(systemB.CanExecute);
            systemB.TryExecuteAndWait();
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
            system.Initialize(world, schedule);

            system.TryExecuteParallelAndWait();
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
            system.Initialize(world);

            system.ExecuteAndWait();
            Assert.Equal(64, system.EntityCount);

            system.Filter = new EntityFilter(new Type[] {
                typeof(TestComponentA)
            });
            system.ExecuteAndWait();
            Assert.Equal(32, system.EntityCount);

            system.Filter = new EntityFilter(
                new Type[] {
                    typeof(TestComponentA)
                },
                new Type[] {
                    typeof(TestComponentB)
                }
            );
            system.TryExecuteAndWait();
            Assert.Equal(16, system.EntityCount);
        }

        [Fact]
        public void OnTerminate() {
            EntityWorld world = new EntityWorld();

            for (int i = 0; i < 16; i++)
                world.NewEntity();

            TestSystemA system = new TestSystemA();
            system.Initialize(world);

            system.ExecuteAndWait();
            system.Dispose();

            Assert.True(system.IsDestroyed);
            Assert.True(system.IsTerminated);
        }

        [Fact]
        public void Initialize() {
            EntityWorld world = new EntityWorld();

            TestSystemB system = new TestSystemB();

            system.Initialize(world);
            Assert.Throws<InvalidOperationException>(() => system.Initialize(world));

            system.Dispose();
            Assert.Throws<InvalidOperationException>(() => system.Initialize(world));
        }

    }
}
