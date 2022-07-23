using NoiseEngine.Jobs;
using System.Collections.Concurrent;

namespace NoiseEngine.Tests.Jobs;

public class ComponentsStorageTests {

    [Fact]
    public void AddStorage() {
        EntityWorld world = new EntityWorld();
        ConcurrentDictionary<Entity, TestComponentA> dictionaryA = world.ComponentsStorage.AddStorage<TestComponentA>();
        ConcurrentDictionary<Entity, TestComponentA> dictionaryB = world.ComponentsStorage.AddStorage<TestComponentA>();

        Assert.Equal(dictionaryA, dictionaryB);
    }

    [Fact]
    public void GetStorage() {
        EntityWorld world = new EntityWorld();
        ConcurrentDictionary<Entity, TestComponentA> dictionaryA = world.ComponentsStorage.AddStorage<TestComponentA>();
        ConcurrentDictionary<Entity, TestComponentA> dictionaryB = world.ComponentsStorage.GetStorage<TestComponentA>();

        Assert.Equal(dictionaryA, dictionaryB);
    }

    [Fact]
    public void AddComponent() {
        EntityWorld world = new EntityWorld();
        Entity entity = world.NewEntity();

        world.ComponentsStorage.AddComponent(entity, new TestComponentA());
    }

    [Fact]
    public void RemoveComponent() {
        EntityWorld world = new EntityWorld();
        Entity entity = world.NewEntity();

        world.ComponentsStorage.AddComponent(entity, new TestComponentA());
        world.ComponentsStorage.RemoveComponent<TestComponentA>(entity);
    }

    [Fact]
    public void SetComponent() {
        EntityWorld world = new EntityWorld();
        Entity entity = world.NewEntity();

        TestComponentA componentA = new TestComponentA() {
            A = 33
        };
        TestComponentA componentB = new TestComponentA() {
            A = 1
        };

        world.ComponentsStorage.AddComponent(entity, componentA);
        Assert.Equal(componentA.A, world.ComponentsStorage.GetComponent<TestComponentA>(entity).A);

        world.ComponentsStorage.SetComponent(entity, componentB);
        Assert.Equal(componentB.A, world.ComponentsStorage.GetComponent<TestComponentA>(entity).A);
    }

    [Fact]
    public void GetComponent() {
        EntityWorld world = new EntityWorld();
        Entity entity = world.NewEntity();

        TestComponentA component = new TestComponentA() {
            A = 5
        };

        world.ComponentsStorage.AddComponent(entity, component);
        Assert.Equal(component.A, world.ComponentsStorage.GetComponent<TestComponentA>(entity).A);
    }

    [Fact]
    public void PopComponent() {
        EntityWorld world = new EntityWorld();
        Entity entity = world.NewEntity();

        TestComponentA component = new TestComponentA() {
            A = 5
        };

        world.ComponentsStorage.AddComponent(entity, component);
        Assert.Equal(component.A, ((TestComponentA)world.ComponentsStorage.PopComponent(
            entity, typeof(TestComponentA))).A);
        Assert.False(entity.Has<TestComponentA>(world));
    }

}
