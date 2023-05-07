using NoiseEngine.Jobs2;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using NoiseEngine.Tests.Jobs2.Affective;
using System;
using System.Threading;

namespace NoiseEngine.Tests.Jobs2;

public class EntityObserverTest : ApplicationTestEnvironment, IDisposable {

    private readonly AutoResetEvent resetEvent = new AutoResetEvent(false);
    private bool invoked;

    public EntityObserverTest(ApplicationFixture fixture) : base(fixture) {
    }

    [Fact]
    public void Changed() {
        EntityObserverLifetime lifetimeA = EntityWorld.AddObserver<MockComponentE, MockComponentB>(ChangedSystem);
        EntityObserverLifetime lifetimeB = EntityWorld.AddObserver<MockComponentB>(ChangedCommands);
        using Entity entity = EntityWorld.Spawn(new MockComponentE(5), MockComponentB.TestValueA);

        using TestSystemB system = new TestSystemB();
        EntityWorld.AddSystem(system);
        system.ExecuteAndWait();

        Assert.True(invoked);
        if (!resetEvent.WaitOne(1000))
            throw new TimeoutException("The observer was not invoked.");

        lifetimeA.Dispose();
        lifetimeB.Dispose();

        Assert.Empty(EntityWorld.changedObservers);
        foreach (Archetype archetype in EntityWorld.Archetypes) {
            foreach (ChangedObserverContext[] context in archetype.ChangedObserversLookup.Values)
                Assert.Empty(context);
        }
    }

    [Fact]
    public void ChangedWithChangedAffectiveArchetype() {
        using EntityObserverLifetime lifetimeA =
            EntityWorld.AddObserver<MockComponentE, MockAffectiveComponentA>(ChangedSystemAffective);
        using EntityObserverLifetime lifetimeB =
            EntityWorld.AddObserver<MockComponentB, MockAffectiveComponentA>(ChangedCommandsAffective);
        using Entity entity = EntityWorld.Spawn(
            new MockComponentE(15), MockComponentB.TestValueA, MockAffectiveComponentA.Medium
        );

        using TestSystemC system = new TestSystemC();
        EntityWorld.AddSystem(system);
        system.ExecuteAndWait();

        Assert.True(invoked);
        if (!resetEvent.WaitOne(1000))
            throw new TimeoutException("The observer was not invoked.");
    }

    private void ChangedSystem(
        Entity entity, SystemCommands commands, Changed<MockComponentE> changed, MockComponentB componentA
    ) {
        Assert.Equal(5, changed.Old.Value);
        Assert.Equal(10, changed.Current.Value);
        Assert.Equal(MockComponentB.TestValueA, componentA);
        invoked = true;

        commands.GetEntity(entity).Insert(MockComponentB.TestValueB);
    }

    private void ChangedCommands(Entity entity, Changed<MockComponentB> changed) {
        Assert.Equal(MockComponentB.TestValueA, changed.Old);
        Assert.Equal(MockComponentB.TestValueB, changed.Current);
        resetEvent.Set();
    }

    private void ChangedSystemAffective(
        Entity entity, SystemCommands commands, Changed<MockComponentE> changed, MockAffectiveComponentA componentA
    ) {
        Assert.Equal(15, changed.Old.Value);
        Assert.Equal(30, changed.Current.Value);
        Assert.Equal(MockAffectiveComponentA.High, componentA);
        invoked = true;

        commands.GetEntity(entity).Insert(MockComponentB.TestValueB).Insert(MockAffectiveComponentA.Medium);
    }

    private void ChangedCommandsAffective(
        Entity entity, Changed<MockComponentB> changed, MockAffectiveComponentA componentA
    ) {
        Assert.Equal(MockComponentB.TestValueA, changed.Old);
        Assert.Equal(MockComponentB.TestValueB, changed.Current);
        Assert.Equal(MockAffectiveComponentA.Medium, componentA);
        resetEvent.Set();
    }

    public void Dispose() {
        resetEvent.Dispose();
    }

}
