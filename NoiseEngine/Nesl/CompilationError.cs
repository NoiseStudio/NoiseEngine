using NoiseEngine.Nesl.CompilerTools;
using NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;

namespace NoiseEngine.Nesl;

public struct CompilationError {

    public CompilationErrorType Type { get; }
    public string Path { get; }
    public uint Line { get; }
    public uint Column { get; }
    public CompilationErrorSeverity Severity { get; }

    internal CompilationError(Token token, CompilationErrorType type) {
        Type = type;
        Path = token.Path;
        Line = token.Line;
        Column = token.Column;

        Severity = Type.GetCustomAttribute<CompilationErrorTypeAttribute>().Severity;
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() {
        return $"{Path}({Line},{Column}): {Severity.ToString().ToLower()} NESL{(uint)Type:D4}: {Type}";
    }

}
