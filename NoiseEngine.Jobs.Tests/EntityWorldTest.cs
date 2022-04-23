﻿using Xunit;
using System;
using System.Collections.Generic;

namespace NoiseEngine.Jobs.Tests {
    public class EntityWorldTest {

        [Fact]
        public void NewEntity() {
            EntityWorld world = new EntityWorld();
            Assert.NotEqual(Entity.Empty, world.NewEntity());
            Assert.NotEqual(world.NewEntity(), world.NewEntity());
        }

        [Fact]
        public void NewEntityT1() {
            EntityWorld world = new EntityWorld();

            Entity entity = world.NewEntity(new TestComponentA());
            Assert.True(entity.Has<TestComponentA>(world));
        }

        [Fact]
        public void NewEntityT2() {
            EntityWorld world = new EntityWorld();

            Entity entity = world.NewEntity(new TestComponentA(), new TestComponentB());
            Assert.True(entity.Has<TestComponentA>(world));
            Assert.True(entity.Has<TestComponentB>(world));
        }

        [Fact]
        public void AddSystem() {
            EntityWorld world = new EntityWorld();

            TestSystemB system = new TestSystemB();
            world.AddSystem(system);
            Assert.Throws<InvalidOperationException>(() => world.AddSystem(system));
        }

        [Fact]
        public void RemoveSystem() {
            EntityWorld world = new EntityWorld();

            TestSystemB system = new TestSystemB();
            world.AddSystem(system);
            world.RemoveSystem(system);
            world.RemoveSystem(system);
        }

        [Fact]
        public void HasSystem() {
            EntityWorld world = new EntityWorld();
            TestSystemB system = new TestSystemB();

            Assert.False(world.HasSystem(system));

            world.AddSystem(system);
            Assert.True(world.HasSystem(system));
        }

        [Fact]
        public void HasAnySystem() {
            EntityWorld world = new EntityWorld();

            Assert.False(world.HasAnySystem<TestSystemB>());

            world.AddSystem(new TestSystemB());
            Assert.True(world.HasAnySystem<TestSystemB>());
        }

        [Fact]
        public void GetSystem() {
            EntityWorld world = new EntityWorld();
            TestSystemB system = new TestSystemB();
            world.AddSystem(system);

            //Assert.Equal(system, world.GetSystem<TestSystemB>());
        }

        [Fact]
        public void GetSystems() {
            EntityWorld world = new EntityWorld();
            TestSystemB system = new TestSystemB();
            world.AddSystem(system);

            TestSystemB[] systems = world.GetSystems<TestSystemB>();
            Assert.Single(systems);

            Assert.NotStrictEqual(systems, world.GetSystems<TestSystemB>());
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

        [Fact]
        public void GetEntityGroup() {
            EntityWorld world = new EntityWorld();
            Entity entity = world.NewEntity();

            EntityGroup groupA = world.GetEntityGroup(entity);
            Assert.Equal(groupA, world.GetEntityGroup(entity));

            entity.Add(world, new TestComponentA());
            EntityGroup groupB = world.GetEntityGroup(entity);

            Assert.NotEqual(groupA, groupB);
        }

        [Fact]
        public void SetEntityGroup() {
            EntityWorld world = new EntityWorld();
            Entity entity = world.NewEntity();

            EntityGroup groupA = world.GetEntityGroup(entity);

            entity.Add(world, new TestComponentA());
            EntityGroup groupB = world.GetEntityGroup(entity);

            Assert.Equal(groupB, world.GetEntityGroup(entity));
            Assert.NotEqual(groupA, groupB);

            world.SetEntityGroup(entity, groupA);
            Assert.Equal(groupA, world.GetEntityGroup(entity));
        }

    }
}
