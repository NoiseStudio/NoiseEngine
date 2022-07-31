using NoiseEngine.Jobs;
using System;
using System.Collections.Generic;

namespace NoiseEngine.Tests.Jobs;

[Collection(nameof(JobsCollection))]
public class EntityWorldTest {

    private JobsFixture Fixture { get; }

    public EntityWorldTest(JobsFixture fixture) {
        Fixture = fixture;
    }

    [Fact]
    public void NewEntity() {
        Assert.NotEqual(Entity.Empty, Fixture.EntityWorld.NewEntity());
        Assert.NotEqual(Fixture.EntityWorld.NewEntity(), Fixture.EntityWorld.NewEntity());
    }

    [Fact]
    public void NewEntityT1() {
        Entity entity = Fixture.EntityWorld.NewEntity(new TestComponentA());
        Assert.True(entity.Has<TestComponentA>(Fixture.EntityWorld));
    }

    [Fact]
    public void NewEntityT2() {
        Entity entity = Fixture.EntityWorld.NewEntity(new TestComponentA(), new TestComponentB());
        Assert.True(entity.Has<TestComponentA>(Fixture.EntityWorld));
        Assert.True(entity.Has<TestComponentB>(Fixture.EntityWorld));
    }

    [Fact]
    public void HasSystem() {
        using TestSystemB system = new TestSystemB();

        Assert.False(Fixture.EntityWorld.HasSystem(system));

        system.Initialize(Fixture.EntityWorld);
        Assert.True(Fixture.EntityWorld.HasSystem(system));
    }

    [Fact]
    public void HasAnySystem() {
        using EntityWorld world = new EntityWorld();
        Assert.False(world.HasAnySystem<TestSystemB>());

        TestSystemB system = new TestSystemB();
        system.Initialize(world);

        Assert.True(world.HasAnySystem<TestSystemB>());
    }

    [Fact]
    public void GetSystem() {
        using TestSystemB system = new TestSystemB();
        system.Initialize(Fixture.EntityWorld);

        Assert.Equal(system, Fixture.EntityWorld.GetSystem<TestSystemB>());
    }

    [Fact]
    public void GetSystems() {
        using TestSystemB system = new TestSystemB();
        system.Initialize(Fixture.EntityWorld);

        IReadOnlyList<TestSystemB> systems = Fixture.EntityWorld.GetSystems<TestSystemB>();
        Assert.Single(systems);

        Assert.NotStrictEqual(systems, Fixture.EntityWorld.GetSystems<TestSystemB>());
    }

    [Fact]
    public void GetGroupFromComponents() {
        EntityGroup group0 = Fixture.EntityWorld.GetGroupFromComponents(new List<Type>() {
            typeof(string), typeof(int), typeof(long)
        });
        EntityGroup group1 = Fixture.EntityWorld.GetGroupFromComponents(new List<Type>() {
            typeof(long), typeof(string), typeof(int)
        });

        Assert.Equal(group0, group1);

        group0 = Fixture.EntityWorld.GetGroupFromComponents(new List<Type>());
        Assert.Equal(0, group0.GetHashCode());
    }

    [Fact]
    public void GetEntityGroup() {
        Entity entity = Fixture.EntityWorld.NewEntity();

        EntityGroup groupA = Fixture.EntityWorld.GetEntityGroup(entity);
        Assert.Equal(groupA, Fixture.EntityWorld.GetEntityGroup(entity));

        entity.Add(Fixture.EntityWorld, new TestComponentA());
        EntityGroup groupB = Fixture.EntityWorld.GetEntityGroup(entity);

        Assert.NotEqual(groupA, groupB);
    }

    [Fact]
    public void SetEntityGroup() {
        Entity entity = Fixture.EntityWorld.NewEntity();

        EntityGroup groupA = Fixture.EntityWorld.GetEntityGroup(entity);

        entity.Add(Fixture.EntityWorld, new TestComponentA());
        EntityGroup groupB = Fixture.EntityWorld.GetEntityGroup(entity);

        Assert.Equal(groupB, Fixture.EntityWorld.GetEntityGroup(entity));
        Assert.NotEqual(groupA, groupB);

        Fixture.EntityWorld.SetEntityGroup(entity, groupA);
        Assert.Equal(groupA, Fixture.EntityWorld.GetEntityGroup(entity));
    }

    [Fact]
    public void Destroy() {
        using EntityWorld world = new EntityWorld();

        for (int i = 0; i < 16; i++)
            world.NewEntity();

        TestSystemA system = new TestSystemA();
        system.Initialize(world);

        system.ExecuteAndWait();

        world.Dispose();

        Assert.True(system.IsDestroyed);
        Assert.True(system.IsTerminated);

        Assert.Throws<InvalidOperationException>(() => new TestSystemA().Initialize(world));
    }

}
