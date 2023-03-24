namespace NoiseEngine.Nesl.CompilerTools;

internal readonly record struct Token(
    string Path,
    uint Line,
    uint Column,
    TokenType Type,
    int Length,
    string? Value
) {

    public CodePointer Pointer => new CodePointer(Path, Line, Column);

}
