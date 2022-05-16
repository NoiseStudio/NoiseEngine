using Xunit;

namespace NoiseEngine.Jobs.Tests {
    public class EntityQueryT1Test {

        [Fact]
        public void Foreach() {
            EntityWorld world = new EntityWorld();
            EntityQuery<TestComponentA> query = new EntityQuery<TestComponentA>(world);

            world.NewEntity();
            world.NewEntity(new TestComponentA());
            world.NewEntity(new TestComponentB());
            world.NewEntity(new TestComponentA(), new TestComponentB());

            int count = 0;
            foreach ((Entity, TestComponentA) element in query)
                count++;

            Assert.Equal(2, count);
        }

    }
}
