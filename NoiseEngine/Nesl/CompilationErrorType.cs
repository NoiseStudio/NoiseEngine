using NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;

namespace NoiseEngine.Nesl;

public enum CompilationErrorType {
    [CompilationErrorType(CompilationErrorSeverity.Error)]
    None = 0,
    [CompilationErrorType(CompilationErrorSeverity.Error)]
    UnexpectedExpression,
    [CompilationErrorType(CompilationErrorSeverity.Error)]
    ExpectedTypeIdentifier,
    [CompilationErrorType(CompilationErrorSeverity.Error)]
    MissingName,
    [CompilationErrorType(CompilationErrorSeverity.Error)]
    ExpectedUsing,
    [CompilationErrorType(CompilationErrorSeverity.Error)]
    ExpectedReturn,
    [CompilationErrorType(CompilationErrorSeverity.Error)]
    ExpectedSemicolon,
    [CompilationErrorType(CompilationErrorSeverity.Error)]
    ExpectedComma,
    [CompilationErrorType(CompilationErrorSeverity.Error)]
    ExpectedOpeningRoundBracket,
    [CompilationErrorType(CompilationErrorSeverity.Error)]
    ExpectedClosingRoundBracket,
    [CompilationErrorType(CompilationErrorSeverity.Error)]
    ExpectedOpeningCurlyBracket,
    [CompilationErrorType(CompilationErrorSeverity.Error)]
    ExpectedClosingCurlyBracket,
    [CompilationErrorType(CompilationErrorSeverity.Error)]
    ExpectedOpeningAngleBracket,
    [CompilationErrorType(CompilationErrorSeverity.Error)]
    ExpectedClosingAngleBracket,
    [CompilationErrorType(CompilationErrorSeverity.Error)]
    ExpectedOpeningSquareBracket,
    [CompilationErrorType(CompilationErrorSeverity.Error)]
    ExpectedClosingSquareBracket,
    [CompilationErrorType(CompilationErrorSeverity.Error)]
    ExpectedOperator,
    [CompilationErrorType(CompilationErrorSeverity.Error)]
    ExpectedValue,
    [CompilationErrorType(CompilationErrorSeverity.Error)]
    ExpectedTypeKind,
    [CompilationErrorType(CompilationErrorSeverity.Error)]
    ExpectedExplicitCastNotValue,
    [CompilationErrorType(CompilationErrorSeverity.Error)]
    ExpectedExplicitCastNotExpression,
    [CompilationErrorType(CompilationErrorSeverity.Error)]
    InvalidTypeKind,
    [CompilationErrorType(CompilationErrorSeverity.Warning)]
    UsingAlreadyExists,
    [CompilationErrorType(CompilationErrorSeverity.Error)]
    TypeAlreadyExists,
    [CompilationErrorType(CompilationErrorSeverity.Error)]
    FieldAlreadyExists,
    [CompilationErrorType(CompilationErrorSeverity.Error)]
    MethodAlreadyExists,
    [CompilationErrorType(CompilationErrorSeverity.Error)]
    ParameterAlreadyExists,
    [CompilationErrorType(CompilationErrorSeverity.Error)]
    TypeNotFound,
    [CompilationErrorType(CompilationErrorSeverity.Error)]
    ConstructorNotFound,
    [CompilationErrorType(CompilationErrorSeverity.Error)]
    UsingGenericNotAllowed,
}
