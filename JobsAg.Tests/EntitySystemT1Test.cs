using Xunit;

namespace NoiseStudio.JobsAg.Tests {
    public class EntitySystemT1Test {

        [Fact]
        public void Test() {
            EntityWorld world = new EntityWorld();

            Entity entityA = world.NewEntity(new TestComponentA());
            Entity entityB = world.NewEntity(new TestComponentA());
            Entity entityC = world.NewEntity(new TestComponentA());

            TestSystemA system = new TestSystemA();
            world.AddSystem(system);

            Assert.Equal(0, entityB.Get<TestComponentA>(world).A);

            system.Execute();
            Assert.Equal(1, entityB.Get<TestComponentA>(world).A);

            world.DisableSystem<TestSystemA>();
            world.EnableSystem<TestSystemA>();

            system.Execute();
            Assert.Equal(101, entityB.Get<TestComponentA>(world).A);
        }

    }
}
