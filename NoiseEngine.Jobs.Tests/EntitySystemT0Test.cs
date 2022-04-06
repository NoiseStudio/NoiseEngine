using Xunit;

namespace NoiseEngine.Jobs.Tests {
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

            system.ExecuteAndWait();
            Assert.Equal(3, system.C);

            system.Enabled = false;
            system.Enabled = true;

            system.ExecuteAndWait();
            Assert.Equal(107, system.C);
        }

    }
}
