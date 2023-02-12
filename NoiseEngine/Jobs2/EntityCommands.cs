using NoiseEngine.Jobs2.Commands;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Jobs2;

public readonly ref struct EntityCommands {

    public readonly SystemCommands Commands { get; }
    public readonly Entity Entity { get; }

    internal EntityCommands(SystemCommands commands, Entity entity) {
        Commands = commands;
        Entity = entity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EntityCommands Despawn() {
        Commands.Objects.Add(new EntityDespawnCommand(Entity));
        return this;
    }

}
