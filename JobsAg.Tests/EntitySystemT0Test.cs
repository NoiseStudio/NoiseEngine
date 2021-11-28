using Xunit;

namespace NoiseStudio.JobsAg.Tests {
    public class EntitySystemT0Test {

        [Fact]
        public void Execute() {
            EntityWorld world = new EntityWorld();

            world.NewEntity();
            world.NewEntity(new TestComponentA());
            world.NewEntity(new TestComponentA());

            TestSystemA system = new TestSystemA();
            world.AddSystem(system);

            Assert.Equal(-5, system.C);

            system.Execute();
            Assert.Equal(3, system.C);

            world.DisableSystem<TestSystemA>();
            world.EnableSystem<TestSystemA>();

            system.Execute();
            Assert.Equal(107, system.C);
        }

    }
}
