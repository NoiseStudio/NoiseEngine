using NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;

namespace NoiseEngine.Nesl;

public enum CompilationErrorType {
    [CompilationErrorType(CompilationErrorSeverity.Error)]
    None = 0,
    [CompilationErrorType(CompilationErrorSeverity.Error)]
    UnexpectedExpression,
    [CompilationErrorType(CompilationErrorSeverity.Error)]
    MissingModifiers
}
