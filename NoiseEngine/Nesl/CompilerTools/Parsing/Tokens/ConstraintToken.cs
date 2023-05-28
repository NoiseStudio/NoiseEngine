using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

internal readonly record struct ConstraintToken(
    TypeIdentifierToken GenericParameter, IReadOnlyList<TypeIdentifierToken> Constraints
) : IParserToken<ConstraintToken> {

    public bool IsIgnored => false;
    public int Priority => 0;

    public static bool Parse(
        TokenBuffer buffer, CompilationErrorMode errorMode, [NotNullWhen(true)] out ConstraintToken result,
        out CompilationError error
    ) {
        if (!buffer.TryReadNext(TokenType.Word, out Token word) || word.Value != "where") {
            result = default;
            error = new CompilationError(word, CompilationErrorType.ExpectedWhereKeyword);
            return false;
        }

        if (!TypeIdentifierToken.Parse(buffer, errorMode, out TypeIdentifierToken genericParameter, out error)) {
            result = default;
            return false;
        }

        if (!InheritanceToken.Parse(buffer, errorMode, out InheritanceToken inheritance, out error)) {
            result = default;
            return false;
        }

        result = new ConstraintToken(genericParameter, inheritance.Inheritances);
        error = default;
        return true;
    }

}
