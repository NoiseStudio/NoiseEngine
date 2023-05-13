using NoiseEngine.Collections;
using NoiseEngine.Jobs.Commands;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Jobs;

public ref struct ConditionalEntityCommands {

    private ref ConditionalEntityCommandsInner inner;

    internal ref ConditionalEntityCommandsInner Inner => ref inner;
    internal readonly EntityCommands EntityCommands { get; }
    internal SystemCommandsInner SystemCommandsInner => EntityCommands.SystemCommandsInner;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    internal ConditionalEntityCommands(EntityCommands entityCommands, ref ConditionalEntityCommandsInner inner) {
        EntityCommands = entityCommands;
        this.inner = inner;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public PostConditionalEntityCommands Then() {
        return new PostConditionalEntityCommands(this);
    }

    /*[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public ConditionalEntityCommands Contains<T>() where T : IComponent {
        SystemCommandsInner.AddCommand(new SystemCommand(SystemCommandType.ConditionalEntityContains, null));
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public ConditionalEntityCommands Or() {
        Commands.Add(new SystemCommand(SystemCommandType.ConditionalEntityOr, null));
        Inner.UsedOr = true;
        return this;
    }*/

}
