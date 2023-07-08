using System;
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

        int index = buffer.Index;
        if (!buffer.TryReadNext(TokenType.Colon)) {
            buffer.Index = index;
            result = new ConstraintToken(genericParameter, Array.Empty<TypeIdentifierToken>());
            error = default;
            return true;
        }

        List<TypeIdentifierToken> constraints = new List<TypeIdentifierToken>();
        while (true) {
            if (!TypeIdentifierToken.Parse(buffer, errorMode, out TypeIdentifierToken constraint, out error)) {
                result = default;
                return false;
            }
            constraints.Add(constraint);

            index = buffer.Index;
            if (!buffer.TryReadNext(TokenType.Comma)) {
                buffer.Index = index;
                break;
            }
        }

        result = new ConstraintToken(genericParameter, constraints.ToArray());
        error = default;
        return true;
    }

}
