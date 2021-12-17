using Xunit;
using System;

namespace NoiseEngine.Jobs.Tests {
    public class EntityTest {

        [Fact]
        public void AddComponent() {
            EntityWorld world = new EntityWorld();
            Entity entity = world.NewEntity();

            EntityGroup group = world.GetEntityGroup(entity);
            Assert.Contains(entity, group.entities);

            entity.Add(world, new TestComponentA());
            Assert.Throws<InvalidOperationException>(() => {
                entity.Add(world, new TestComponentA());
            });

            Assert.DoesNotContain(entity, group.entities);
        }

        [Fact]
        public void RemoveComponent() {
            EntityWorld world = new EntityWorld();
            Entity entity = world.NewEntity();

            entity.Add(world, new TestComponentA());

            EntityGroup group = world.GetEntityGroup(entity);
            Assert.Contains(entity, group.entities);

            entity.Remove<TestComponentA>(world);
            Assert.Throws<InvalidOperationException>(() => {
                entity.Remove<TestComponentA>(world);
            });

            Assert.DoesNotContain(entity, group.entities);
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
        public void GetComponent() {
            EntityWorld world = new EntityWorld();
            Entity entity = world.NewEntity();

            entity.Add(world, new TestComponentA() {
                A = 9
            });

            Assert.Equal(9, entity.Get<TestComponentA>(world).A);
        }

        [Fact]
        public void SetComponent() {
            EntityWorld world = new EntityWorld();
            Entity entity = world.NewEntity();

            entity.Add(world, new TestComponentA());
            entity.Set(world, new TestComponentA() {
                A = 6
            });
            Assert.Equal(6, entity.Get<TestComponentA>(world).A);
        }

        [Fact]
        public void Destroy() {
            EntityWorld world = new EntityWorld();
            Entity entity = world.NewEntity();

            entity.Add(world, new TestComponentA());
            entity.Get<TestComponentA>(world);

            EntityGroup group = world.GetEntityGroup(entity);
            Assert.Contains(entity, group.entities);

            entity.Destroy(world);
            Assert.Throws<NullReferenceException>(() => entity.Get<TestComponentA>(world));

            Assert.DoesNotContain(entity, group.entities);
        }

        [Fact]
        public void IsDestroyed() {
            EntityWorld world = new EntityWorld();
            Entity entity = world.NewEntity();

            entity.Add(world, new TestComponentA());
            entity.Get<TestComponentA>(world);

            Assert.False(entity.IsDestroyed(world));
            entity.Destroy(world);
            Assert.True(entity.IsDestroyed(world));
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
