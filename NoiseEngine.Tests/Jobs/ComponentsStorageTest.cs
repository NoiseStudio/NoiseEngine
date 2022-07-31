using NoiseEngine.Jobs;
using System.Collections.Concurrent;

namespace NoiseEngine.Tests.Jobs;

[Collection(nameof(JobsCollection))]
public class ComponentsStorageTest {

    private JobsFixture Fixture { get; }

    public ComponentsStorageTest(JobsFixture fixture) {
        Fixture = fixture;
    }

    [Fact]
    public void AddStorage() {
        ConcurrentDictionary<Entity, TestComponentA> dictionaryA =
            Fixture.EntityWorld.ComponentsStorage.AddStorage<TestComponentA>();
        ConcurrentDictionary<Entity, TestComponentA> dictionaryB =
            Fixture.EntityWorld.ComponentsStorage.AddStorage<TestComponentA>();

        Assert.Equal(dictionaryA, dictionaryB);
    }

    [Fact]
    public void GetStorage() {
        ConcurrentDictionary<Entity, TestComponentA> dictionaryA =
            Fixture.EntityWorld.ComponentsStorage.AddStorage<TestComponentA>();
        ConcurrentDictionary<Entity, TestComponentA> dictionaryB =
            Fixture.EntityWorld.ComponentsStorage.GetStorage<TestComponentA>();

        Assert.Equal(dictionaryA, dictionaryB);
    }

    [Fact]
    public void AddComponent() {
        Entity entity = Fixture.NextEmptyEntity;
        Fixture.EntityWorld.ComponentsStorage.AddComponent(entity, new TestComponentA());
    }

    [Fact]
    public void RemoveComponent() {
        Entity entity = Fixture.NextEmptyEntity;

        Fixture.EntityWorld.ComponentsStorage.AddComponent(entity, new TestComponentA());
        Fixture.EntityWorld.ComponentsStorage.RemoveComponent<TestComponentA>(entity);
    }

    [Fact]
    public void SetComponent() {
        Entity entity = Fixture.NextEmptyEntity;
        TestComponentA componentA = new TestComponentA { A = 33 };
        TestComponentA componentB = new TestComponentA { A = 1 };

        Fixture.EntityWorld.ComponentsStorage.AddComponent(entity, componentA);
        Assert.Equal(componentA.A, Fixture.EntityWorld.ComponentsStorage.GetComponent<TestComponentA>(entity).A);

        Fixture.EntityWorld.ComponentsStorage.SetComponent(entity, componentB);
        Assert.Equal(componentB.A, Fixture.EntityWorld.ComponentsStorage.GetComponent<TestComponentA>(entity).A);
    }

    [Fact]
    public void GetComponent() {
        Entity entity = Fixture.NextEmptyEntity;
        TestComponentA component = new TestComponentA { A = 5 };

        Fixture.EntityWorld.ComponentsStorage.AddComponent(entity, component);
        Assert.Equal(component.A, Fixture.EntityWorld.ComponentsStorage.GetComponent<TestComponentA>(entity).A);
    }

    [Fact]
    public void PopComponent() {
        Entity entity = Fixture.NextEmptyEntity;
        TestComponentA component = new TestComponentA { A = 5 };

        Fixture.EntityWorld.ComponentsStorage.AddComponent(entity, component);
        Assert.Equal(component.A, ((TestComponentA)Fixture.EntityWorld.ComponentsStorage.PopComponent(
            entity, typeof(TestComponentA))).A);
        Assert.False(entity.Has<TestComponentA>(Fixture.EntityWorld));
    }

}
