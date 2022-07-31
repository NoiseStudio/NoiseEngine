using NoiseEngine.Jobs;
using System;

namespace NoiseEngine.Tests.Jobs;

[Collection(nameof(JobsCollection))]
public class EntityTest {

    private JobsFixture Fixture { get; }

    public EntityTest(JobsFixture fixture) {
        Fixture = fixture;
    }

    [Fact]
    public void AddComponent() {
        Entity entity = Fixture.EntityWorld.NewEntity();

        EntityGroup group = Fixture.EntityWorld.GetEntityGroup(entity);
        Assert.Contains(entity, group.Entities);

        entity.Add(Fixture.EntityWorld, new TestComponentA());
        Assert.Throws<InvalidOperationException>(() => {
            entity.Add(Fixture.EntityWorld, new TestComponentA());
        });

        Assert.DoesNotContain(entity, group.Entities);
    }

    [Fact]
    public void RemoveComponent() {
        Entity entity = Fixture.EntityWorld.NewEntity();

        entity.Add(Fixture.EntityWorld, new TestComponentA());

        EntityGroup group = Fixture.EntityWorld.GetEntityGroup(entity);
        Assert.Contains(entity, group.Entities);

        entity.Remove<TestComponentA>(Fixture.EntityWorld);
        Assert.Throws<InvalidOperationException>(() => {
            entity.Remove<TestComponentA>(Fixture.EntityWorld);
        });

        Assert.DoesNotContain(entity, group.Entities);
    }

    [Fact]
    public void HasComponent() {
        Entity entity = Fixture.EntityWorld.NewEntity();

        Assert.False(entity.Has<TestComponentA>(Fixture.EntityWorld));

        entity.Add(Fixture.EntityWorld, new TestComponentA());
        Assert.True(entity.Has<TestComponentA>(Fixture.EntityWorld));
    }

    [Fact]
    public void GetComponent() {
        Entity entity = Fixture.EntityWorld.NewEntity();

        entity.Add(Fixture.EntityWorld, new TestComponentA {
            A = 9
        });

        Assert.Equal(9, entity.Get<TestComponentA>(Fixture.EntityWorld).A);
    }

    [Fact]
    public void SetComponent() {
        Entity entity = Fixture.EntityWorld.NewEntity();

        entity.Add(Fixture.EntityWorld, new TestComponentA());
        entity.Set(Fixture.EntityWorld, new TestComponentA {
            A = 6
        });
        Assert.Equal(6, entity.Get<TestComponentA>(Fixture.EntityWorld).A);
    }

    [Fact]
    public void Destroy() {
        Entity entity = Fixture.EntityWorld.NewEntity();

        entity.Add(Fixture.EntityWorld, new TestComponentA());
        entity.Get<TestComponentA>(Fixture.EntityWorld);

        EntityGroup group = Fixture.EntityWorld.GetEntityGroup(entity);
        Assert.Contains(entity, group.Entities);

        entity.Destroy(Fixture.EntityWorld);
        Assert.True(entity.IsDestroyed(Fixture.EntityWorld));

        Assert.DoesNotContain(entity, group.Entities);
    }

    [Fact]
    public void IsDestroyed() {
        Entity entity = Fixture.EntityWorld.NewEntity();

        entity.Add(Fixture.EntityWorld, new TestComponentA());
        entity.Get<TestComponentA>(Fixture.EntityWorld);

        Assert.False(entity.IsDestroyed(Fixture.EntityWorld));
        entity.Destroy(Fixture.EntityWorld);
        Assert.True(entity.IsDestroyed(Fixture.EntityWorld));
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
