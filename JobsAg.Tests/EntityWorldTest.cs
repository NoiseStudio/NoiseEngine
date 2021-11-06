using Xunit;
using System;
using System.Collections.Generic;

namespace NoiseStudio.JobsAg.Tests {
    public class EntityWorldTest {

        [Fact]
        public void NewEntity() {
            EntityWorld world = new EntityWorld();

            Assert.Equal(new Entity(0), world.NewEntity());
            world.NewEntity();
            Assert.Equal(new Entity(2), world.NewEntity());
        }

        [Fact]
        public void GetGroupFromComponents() {
            EntityWorld world = new EntityWorld();

            EntityGroup group0 = world.GetGroupFromComponents(new List<Type>() { typeof(string), typeof(int), typeof(long) });
            EntityGroup group1 = world.GetGroupFromComponents(new List<Type>() { typeof(long), typeof(string), typeof(int) });
            Assert.Equal(group0, group1);

            group0 = world.GetGroupFromComponents(new List<Type>());
            Assert.Equal(0, group0.GetHashCode());
        }

    }
}
