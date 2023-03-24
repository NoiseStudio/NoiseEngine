namespace NoiseEngine.Nesl.CompilerTools;

internal readonly record struct CodePointer(
    string Path,
    uint Line,
    uint Column
);
