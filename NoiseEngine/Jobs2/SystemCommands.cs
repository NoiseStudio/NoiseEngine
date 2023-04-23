using NoiseEngine.Collections;
using NoiseEngine.Jobs2.Commands;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Jobs2;

public ref struct SystemCommands {

    internal SystemCommandsInner Inner { get; }

    public SystemCommands() : this(4) {
    }

    public SystemCommands(int capacity) {
        Inner = new SystemCommandsInner(new FastList<SystemCommand>(capacity));
    }

    internal SystemCommands(FastList<SystemCommand> commands) {
        Inner = new SystemCommandsInner(commands);
    }

    /// <summary>
    /// Creates <see cref="EntityCommands"/> from given <paramref name="entity"/>.
    /// </summary>
    /// <param name="entity"><see cref="Entity"/> to work.</param>
    /// <returns><see cref="EntityCommands"/> from <paramref name="entity"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EntityCommands GetEntity(Entity entity) {
        EntityCommandsInner inner = new EntityCommandsInner(entity);
        Inner.SetMutationTreeObjectAndAddCommand(inner, new SystemCommand(SystemCommandType.GetEntity, inner));
        return new EntityCommands(this, inner);
    }

}
