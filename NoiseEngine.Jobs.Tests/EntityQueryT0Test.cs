using Xunit;

namespace NoiseEngine.Jobs.Tests {
    public class EntityQueryT0Test {

        [Fact]
        public void Foreach() {
            EntityWorld world = new EntityWorld();
            EntityQuery query = new EntityQuery(world, true);

            world.NewEntity();
            world.NewEntity(new TestComponentA());
            world.NewEntity(new TestComponentB());
            world.NewEntity(new TestComponentA(), new TestComponentB());

            int count = 0;
            foreach (Entity entity in query)
                count++;

            Assert.Equal(4, count);
        }

    }
}
