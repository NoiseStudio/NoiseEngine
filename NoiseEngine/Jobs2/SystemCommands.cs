using NoiseEngine.Collections;
using NoiseEngine.Jobs2.Commands;
using System;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Jobs2;

public ref struct SystemCommands {

    private FastList<SystemCommand>? commands;

    internal FastList<SystemCommand> Commands => commands ?? throw new InvalidOperationException(
        $"This {nameof(SystemCommands)} is already used."
    );

    public SystemCommands() : this(4) {
    }

    public SystemCommands(int capacity) {
        commands = new FastList<SystemCommand>(capacity);
    }

    internal SystemCommands(FastList<SystemCommand> commands) {
        this.commands = commands;
    }

    /// <summary>
    /// Creates <see cref="EntityCommands"/> from given <paramref name="entity"/>.
    /// </summary>
    /// <param name="entity"><see cref="Entity"/> to work.</param>
    /// <returns><see cref="EntityCommands"/> from <paramref name="entity"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EntityCommands GetEntity(Entity entity) {
        EntityCommandsInner inner = new EntityCommandsInner(entity);
        Commands.Add(new SystemCommand(SystemCommandType.GetEntity, inner));
        return new EntityCommands(this, inner);
    }

}
