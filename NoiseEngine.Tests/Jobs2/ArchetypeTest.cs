using NoiseEngine.Jobs2;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using System;
using System.Collections.Generic;

namespace NoiseEngine.Tests.Jobs2;

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

        Assert.True(archetype.TryReadAnyRecord(new Type[] {
            typeof(MockComponentA), typeof(MockComponentC)
        }, out Dictionary<Type, object>? components));
        Assert.Equal(MockComponentA.TestValueA, components![typeof(MockComponentA)]);
        Assert.Equal(MockComponentC.TestValueA, components[typeof(MockComponentC)]);

        SystemCommands commands = new SystemCommands();
        commands.GetEntity(entity).Despawn();
        EntityWorld.ExecuteCommands(commands);
        Assert.Null(entity.chunk);

        Assert.False(archetype.TryReadAnyRecord(new Type[] {
            typeof(MockComponentB), typeof(MockComponentA)
        }, out _));
    }

}
