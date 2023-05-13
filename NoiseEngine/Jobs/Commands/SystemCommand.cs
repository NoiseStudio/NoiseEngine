namespace NoiseEngine.Jobs2.Commands;

internal readonly record struct SystemCommand(SystemCommandType Type, object? Value);
