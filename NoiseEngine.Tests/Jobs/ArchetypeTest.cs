using NoiseEngine.Jobs;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using System;
using System.Collections.Generic;

namespace NoiseEngine.Tests.Jobs;

public class ArchetypeTest : ApplicationTestEnvironment {

    public ArchetypeTest(ApplicationFixture fixture) : base(fixture) {
    }

    [Fact]
    public void TryReadAnyRecord() {
        Entity entity = EntityWorld.Spawn(
            MockComponentA.TestValueA, MockComponentB.TestValueA, MockComponentC.TestValueA
        );

        Assert.NotNull(entity.chunk);
        Archetype archetype = entity.chunk!.Archetype;

        Assert.True(archetype.TryReadAnyRecord(out Dictionary<Type, IComponent>? components));
        Assert.Equal(MockComponentA.TestValueA, components![typeof(MockComponentA)]);
        Assert.Equal(MockComponentC.TestValueA, components[typeof(MockComponentC)]);

        SystemCommands commands = new SystemCommands();
        commands.GetEntity(entity).Despawn();
        EntityWorld.ExecuteCommands(commands);
        Assert.Null(entity.chunk);

        Assert.False(archetype.TryReadAnyRecord(out _));
    }

    [Fact]
    public void PointerAlignment() {
        MockComponentA a = MockComponentA.TestValueB;
        using Entity entity = EntityWorld.Spawn(MockComponentG.TestValueA, a);

        Assert.True(entity.TryGet(out MockComponentG g));
        Assert.Equal(MockComponentG.TestValueA, g);
        Assert.True(entity.TryGet(out MockComponentA? a2));
        Assert.Equal(a, a2!);
    }

}
