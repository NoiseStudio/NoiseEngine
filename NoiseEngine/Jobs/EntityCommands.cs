﻿using NoiseEngine.Jobs.Commands;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Jobs;

public ref struct EntityCommands {

    internal readonly SystemCommands SystemCommands { get; }
    internal EntityCommandsInner Inner { get; }
    internal SystemCommandsInner SystemCommandsInner => SystemCommands.Inner;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    internal EntityCommands(SystemCommands systemCommands, EntityCommandsInner inner) {
        SystemCommands = systemCommands;
        Inner = inner;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public EntityCommands Despawn() {
        SystemCommandsInner.AddCommand(Inner, new SystemCommand(SystemCommandType.EntityDespawn, null));
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public EntityCommands Insert<T>(T component) where T : IComponent {
        SystemCommandsInner.AddCommand(Inner, new SystemCommand(
            SystemCommandType.EntityInsert,
            ((IComponent)component, IAffectiveComponent.GetAffectiveHashCode(component))
        ));
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public EntityCommands Remove<T>() where T : IComponent {
        SystemCommandsInner.AddCommand(Inner, new SystemCommand(SystemCommandType.EntityRemove, typeof(T)));
        return this;
    }

    /*[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public ConditionalEntityCommands When(Entity entity) {
        if (Inner.ConditionalsCount >= Inner.Conditionals.Length)
            Array.Resize(ref Inner.Conditionals, Math.Max(Inner.Conditionals.Length * 2, 1));

        Commands.Add(new SystemCommand(SystemCommandType.EntityWhen, null));

        ref ConditionalEntityCommandsInner inner = ref Inner.Conditionals[Inner.ConditionalsCount++];
        inner = new ConditionalEntityCommandsInner(entity);
        return new ConditionalEntityCommands(this, ref inner);
    }*/

}
