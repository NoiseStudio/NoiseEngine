using NoiseEngine.Collections;
using NoiseEngine.Jobs2.Commands;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Jobs2;

public ref struct ConditionalEntityCommands {

    private ref ConditionalEntityCommandsInner inner;

    internal ref ConditionalEntityCommandsInner Inner => ref inner;
    internal readonly EntityCommands EntityCommands { get; }
    internal FastList<SystemCommand> Commands => EntityCommands.Commands;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    internal ConditionalEntityCommands(EntityCommands entityCommands, ref ConditionalEntityCommandsInner inner) {
        EntityCommands = entityCommands;
        this.inner = inner;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public PostConditionalEntityCommands Then() {
        return new PostConditionalEntityCommands(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public ConditionalEntityCommands Contains<T>() where T : IComponent {
        Commands.Add(new SystemCommand(SystemCommandType.ConditionalEntityContains, null));
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public ConditionalEntityCommands Or() {
        Commands.Add(new SystemCommand(SystemCommandType.ConditionalEntityOr, null));
        Inner.UsedOr = true;
        return this;
    }

}
