namespace NoiseEngine.Jobs.Commands;

internal readonly record struct SystemCommand(SystemCommandType Type, object? Value);
