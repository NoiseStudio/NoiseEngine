using Xunit;
using System;

namespace NoiseStudio.JobsAg.Tests {
    public class EntityTest {

        [Fact]
        public void AddComponent() {
            EntityWorld world = new EntityWorld();
            Entity entity = world.NewEntity();

            entity.Add(world, new TestComponentA());
            Assert.Throws<InvalidOperationException>(() => {
                entity.Add(world, new TestComponentA());
            });
        }

        [Fact]
        public void RemoveComponent() {
            EntityWorld world = new EntityWorld();
            Entity entity = world.NewEntity();

            entity.Add(world, new TestComponentA());

            entity.Remove<TestComponentA>(world);
            Assert.Throws<InvalidOperationException>(() => {
                entity.Remove<TestComponentA>(world);
            });
        }

        [Fact]
        public void HasComponent() {
            EntityWorld world = new EntityWorld();
            Entity entity = world.NewEntity();

            Assert.False(entity.Has<TestComponentA>(world));
            
            entity.Add(world, new TestComponentA());
            Assert.True(entity.Has<TestComponentA>(world));
        }

        [Fact]
        public void GetHashCodeTest() {
            Entity a = new Entity(11);
            Entity b = new Entity(11);
            Entity c = new Entity(69);

            Assert.Equal(a.GetHashCode(), b.GetHashCode());
            Assert.NotEqual(a.GetHashCode(), c.GetHashCode());
        }

        [Fact]
        public void EqualsTest() {
            Entity a = new Entity(420);
            Entity b = new Entity(420);
            Entity c = new Entity(2137);

            Assert.True(a.Equals((object)b));
            Assert.False(a.Equals((object)c));
            Assert.False(b.Equals(null));
            Assert.False(c.Equals((ulong)2137));

            Assert.True(a == b);
            Assert.False(a == c);
            Assert.True(a != c);
        }

        [Fact]
        public void EqualsGenericTest() {
            Entity a = new Entity(36);
            Entity b = new Entity(36);
            Entity c = new Entity(773);

            Assert.True(a.Equals(b));
            Assert.False(a.Equals(c));
        }

    }
}
