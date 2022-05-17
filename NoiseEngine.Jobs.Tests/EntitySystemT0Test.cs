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
            system.Initialize(world);

            Assert.Equal(-5, system.C);

            system.TryExecuteAndWait();
            Assert.Equal(3, system.C);

            system.Enabled = false;
            system.Enabled = true;

            system.TryExecuteAndWait();
            Assert.Equal(107, system.C);
        }

    }
}
