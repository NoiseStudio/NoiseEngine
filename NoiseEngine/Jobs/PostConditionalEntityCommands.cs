using NoiseEngine.Collections;
using NoiseEngine.Jobs.Commands;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Jobs;

public ref struct PostConditionalEntityCommands {

    internal readonly ConditionalEntityCommands ConditionalCommands { get; }
    internal SystemCommandsInner SystemCommandsInner => ConditionalCommands.SystemCommandsInner;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    internal PostConditionalEntityCommands(ConditionalEntityCommands conditionalCommands) {
        ConditionalCommands = conditionalCommands;
    }

    /*[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public PostConditionalEntityCommands Insert<T>(T component) where T : IComponent {
        Commands.Add(new SystemCommand(SystemCommandType.EntityInsert, component));
        return this;
    }

    /*[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public ConditionalEntityCommands When(Entity entity) {
        return ConditionalCommands.EntityCommands.When(entity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public ConditionalEntityCommands Else() {
        Commands.Add(new SystemCommand(SystemCommandType.PostConditionalEntityElse, null));
        return ConditionalCommands;
    }*/

}
