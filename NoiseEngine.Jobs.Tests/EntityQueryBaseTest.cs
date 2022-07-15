using Xunit;

namespace NoiseEngine.Jobs.Tests {
    public class EntityQueryBaseTest {

        [Fact]
        public void ForeachEntities() {
            EntityWorld world = new EntityWorld();
            EntityQuery query = new EntityQuery(world);

            world.NewEntity();
            world.NewEntity(new TestComponentA());
            world.NewEntity(new TestComponentB());
            world.NewEntity(new TestComponentA(), new TestComponentB());

            int count = 0;
            foreach (Entity entity in query.Entities)
                count++;

            Assert.Equal(4, count);
        }

    }
}
