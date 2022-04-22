using Xunit;
using System;
using System.Collections.Generic;

namespace NoiseEngine.Jobs.Tests {
    public class EntityGroupTest {

        [Fact]
        public void GetHashCodeTest() {
            EntityGroup groupA = new EntityGroup(5, EntityWorld.Empty, new List<Type>() { typeof(string), typeof(int) });
            Assert.Equal(5, groupA.GetHashCode());
        }

        [Fact]
        public void AddRemoveEntity() {
            EntityGroup groupA = new EntityGroup(5, EntityWorld.Empty, new List<Type>());

            Entity entity = new Entity(0);
            groupA.AddEntity(entity);

            Assert.Single(groupA.Entities);
            groupA.RemoveEntity(entity);
            Assert.Empty(groupA.Entities);
        }

        [Fact]
        public void CompareSortedComponents() {
            EntityGroup groupA = new EntityGroup(5, EntityWorld.Empty, new List<Type>() { typeof(string), typeof(int) });
            Assert.True(groupA.CompareSortedComponents(new List<Type>() { typeof(string), typeof(int) }));
        }

        [Fact]
        public void HasComponent() {
            EntityGroup groupA = new EntityGroup(5, EntityWorld.Empty, new List<Type>() { typeof(string), typeof(int) });
            Assert.True(groupA.HasComponent(typeof(string)));
            Assert.False(groupA.HasComponent(typeof(float)));
        }

    }
}
