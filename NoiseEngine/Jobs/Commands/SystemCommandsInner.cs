using NoiseEngine.Collections;
using System;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Jobs.Commands;

internal sealed class SystemCommandsInner {

    private FastList<SystemCommand>? commands;
    private object? mutationTreeObject;

    public FastList<SystemCommand> Commands => commands ?? throw new InvalidOperationException(
        $"This {nameof(SystemCommands)} is already used."
    );

    public SystemCommandsInner(FastList<SystemCommand> commands) {
        this.commands = commands;
    }

    public void Consume() {
        commands = null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void SetMutationTreeObject(object obj) {
        if (ApplicationJitConsts.IsDebugMode)
            mutationTreeObject = obj;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void AddCommand(object obj, SystemCommand command) {
        if (ApplicationJitConsts.IsDebugMode && mutationTreeObject != obj) {
            throw new InvalidOperationException(
                $"This {nameof(SystemCommands)} is already used by another object."
            );
        }

        Commands.Add(command);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void SetMutationTreeObjectAndAddCommand(object obj, SystemCommand command) {
        SetMutationTreeObject(obj);
        Commands.Add(command);
    }

}
