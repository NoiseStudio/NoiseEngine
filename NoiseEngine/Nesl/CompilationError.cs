using NoiseEngine.Nesl.CompilerTools;
using NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;

namespace NoiseEngine.Nesl;

public readonly struct CompilationError {

    private readonly object received;

    public CompilationErrorType Type { get; }
    public string Path { get; }
    public uint Line { get; }
    public uint Column { get; }
    public CompilationErrorSeverity Severity { get; }

    internal CompilationError(CodePointer pointer, CompilationErrorType type, object received) {
        this.received = received;

        Type = type;
        Path = pointer.Path;
        Line = pointer.Line;
        Column = pointer.Column;

        Severity = Type.GetCustomAttribute<CompilationErrorTypeAttribute>().Severity;
    }

    internal CompilationError(Token token, CompilationErrorType type) : this(
        token.Pointer, type, token.Value ?? token.Type.ToString()
    ) {
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() {
        return $"{Path}({Line},{Column}): {
            Severity.ToString().ToLower()
        } NESL{(uint)Type:D4}: {Type} (received {received})";
    }

}
