using NoiseEngine.Jobs;
using System;

namespace NoiseEngine.Tests.Jobs;

[Collection(nameof(JobsCollection))]
public class EntitySystemBaseTest {

    private JobsFixture Fixture { get; }

    public EntitySystemBaseTest(JobsFixture fixture) {
        Fixture = fixture;
    }

    [Fact]
    public void ExecuteMultithread() {
        using EntityWorld world = Fixture.CreateEntityWorld();

        world.NewEntity(new TestComponentA());
        Entity entity = world.NewEntity(new TestComponentA());
        world.NewEntity(new TestComponentA());

        using TestSystemB system = new TestSystemB();
        Assert.Throws<InvalidOperationException>(() => system.TryExecuteParallelAndWait());

        system.Initialize(world, Fixture.EntitySchedule);

        Assert.Equal(0, entity.Get<TestComponentA>(world).A);

        system.TryExecuteParallelAndWait();
        Assert.Equal(1, entity.Get<TestComponentA>(world).A);

        system.Enabled = false;
        system.Enabled = true;

        system.TryExecuteParallelAndWait();
        Assert.Equal(105, entity.Get<TestComponentA>(world).A);
    }

    [Fact]
    public void Enable() {
        Fixture.EntityWorld.NewEntity();
        Fixture.EntityWorld.NewEntity(new TestComponentA());
        Fixture.EntityWorld.NewEntity(new TestComponentA());

        using TestSystemA system = new TestSystemA();
        system.Initialize(Fixture.EntityWorld);

        system.TryExecuteAndWait();

        system.Enabled = false;
        Assert.False(system.TryExecuteAndWait());

        system.Enabled = true;
        system.TryExecuteAndWait();
    }

    [Fact]
    public void Dependency() {
        Fixture.EntityWorld.NewEntity(new TestComponentA(), new TestComponentB());
        Fixture.EntityWorld.NewEntity(new TestComponentA(), new TestComponentB());
        Fixture.EntityWorld.NewEntity(new TestComponentA(), new TestComponentB());

        using TestSystemA systemA = new TestSystemA();
        using TestSystemB systemB = new TestSystemB();
        systemA.Initialize(Fixture.EntityWorld, Fixture.EntitySchedule);
        systemB.Initialize(Fixture.EntityWorld, Fixture.EntitySchedule);

        systemB.TryExecuteAndWait();
        systemB.AddDependency(systemA);

        systemB.TryExecuteParallelAndWait();
        Assert.False(systemB.TryExecute());

        systemA.TryExecuteAndWait();
        systemA.TryExecuteAndWait();

        systemB.TryExecuteAndWait();
        Assert.False(systemB.TryExecuteAndWait());
    }

    [Fact]
    public void CanExecute() {
        Fixture.EntityWorld.NewEntity(new TestComponentA(), new TestComponentB());
        Fixture.EntityWorld.NewEntity(new TestComponentA(), new TestComponentB());
        Fixture.EntityWorld.NewEntity(new TestComponentA(), new TestComponentB());

        using TestSystemA systemA = new TestSystemA();
        using TestSystemB systemB = new TestSystemB();
        systemA.Initialize(Fixture.EntityWorld);
        systemB.Initialize(Fixture.EntityWorld);

        systemB.TryExecuteAndWait();
        systemB.AddDependency(systemA);

        Assert.True(systemB.CanExecute);
        systemB.TryExecuteAndWait();
        Assert.False(systemB.CanExecute);

        Assert.True(systemA.CanExecute);
    }

    [Fact]
    public void ThreadId() {
        using EntityWorld world = Fixture.CreateEntityWorld();

        int threadCount = 16;
        for (int i = 1; i <= threadCount; i++) {
            world.NewEntity(new TestComponentA() {
                A = 2 * (int)Math.Pow(2, i) + 4
            });
        }

        using TestSystemThreadId system = new TestSystemThreadId();
        system.Initialize(world, Fixture.EntitySchedule);

        system.TryExecuteParallelAndWait();
        Assert.Equal(262204 / threadCount, system.AverageTestComponentAAValue);
    }

    [Fact]
    public void Filter() {
        using EntityWorld world = Fixture.CreateEntityWorld();

        for (int i = 0; i < 16; i++) {
            world.NewEntity();
            world.NewEntity(new TestComponentA());
            world.NewEntity(new TestComponentB());
            world.NewEntity(new TestComponentA(), new TestComponentB());
        }

        using TestSystemCounter system = new TestSystemCounter();
        system.Initialize(world);

        system.ExecuteAndWait();
        Assert.Equal(64, system.EntityCount);

        system.Filter = new EntityFilter(new Type[] {
            typeof(TestComponentA)
        });
        system.ExecuteAndWait();
        Assert.Equal(32, system.EntityCount);

        system.Filter = new EntityFilter(
            new Type[] {
                typeof(TestComponentA)
            },
            new Type[] {
                typeof(TestComponentB)
            }
        );
        system.TryExecuteAndWait();
        Assert.Equal(16, system.EntityCount);
    }

    [Fact]
    public void OnTerminate() {
        for (int i = 0; i < 16; i++)
            Fixture.EntityWorld.NewEntity();

        TestSystemA system = new TestSystemA();
        system.Initialize(Fixture.EntityWorld);

        system.ExecuteAndWait();
        system.Dispose();

        Assert.True(system.IsDestroyed);
        Assert.True(system.IsTerminated);
    }

    [Fact]
    public void Initialize() {
        TestSystemB system = new TestSystemB();

        system.Initialize(Fixture.EntityWorld);
        Assert.Throws<InvalidOperationException>(() => system.Initialize(Fixture.EntityWorld));

        system.Dispose();
        Assert.Throws<InvalidOperationException>(() => system.Initialize(Fixture.EntityWorld));
    }

}
