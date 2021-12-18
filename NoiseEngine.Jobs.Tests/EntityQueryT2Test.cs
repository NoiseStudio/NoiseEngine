using Xunit;

namespace NoiseEngine.Jobs.Tests {
    public class EntityQueryT2Test {

        [Fact]
        public void Foreach() {
            EntityWorld world = new EntityWorld();
            EntityQuery<TestComponentA, TestComponentB> query = new EntityQuery<TestComponentA, TestComponentB>(world);

            world.NewEntity();
            world.NewEntity(new TestComponentA());
            world.NewEntity(new TestComponentB());
            world.NewEntity(new TestComponentA(), new TestComponentB());

            int count = 0;
            foreach ((Entity, TestComponentA, TestComponentB) element in query)
                count++;

            Assert.Equal(1, count);
        }

    }
}
