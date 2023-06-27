using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

internal readonly record struct InheritanceConstraintToken(
    TypeIdentifierToken GenericParameter, IReadOnlyList<TypeIdentifierToken> Inheritance
) : IParserToken<InheritanceConstraintToken> {

    public bool IsIgnored => false;
    public int Priority => 0;

    public static bool Parse(
        TokenBuffer buffer, CompilationErrorMode errorMode, [NotNullWhen(true)] out InheritanceConstraintToken result,
        out CompilationError error
    ) {
        if (!buffer.TryReadNext(TokenType.Word, out Token word) || word.Value != "for") {
            result = default;
            error = new CompilationError(word, CompilationErrorType.ExpectedForKeyword);
            return false;
        }

        if (!TypeIdentifierToken.Parse(buffer, errorMode, out TypeIdentifierToken genericParameter, out error)) {
            result = default;
            return false;
        }

        int index = buffer.Index;
        if (!buffer.TryReadNext(TokenType.Colon)) {
            buffer.Index = index;
            result = new InheritanceConstraintToken(genericParameter, Array.Empty<TypeIdentifierToken>());
            error = default;
            return true;
        }

        if (!buffer.TryReadNext(TokenType.SquareBracketOpening, out Token token)) {
            result = default;
            error = new CompilationError(token, CompilationErrorType.ExpectedOpeningSquareBracket);
            return false;
        }

        List<TypeIdentifierToken> inheritances = new List<TypeIdentifierToken>();
        while (true) {
            if (!TypeIdentifierToken.Parse(buffer, errorMode, out TypeIdentifierToken inheritance, out error)) {
                result = default;
                return false;
            }
            inheritances.Add(inheritance);

            if (buffer.TryReadNext(out token)) {
                if (token.Type == TokenType.Comma)
                    continue;
                if (token.Type == TokenType.SquareBracketClosing)
                    break;
            }

            result = default;
            error = new CompilationError(token, CompilationErrorType.ExpectedClosingSquareBracket);
            return false;
        }

        result = new InheritanceConstraintToken(genericParameter, inheritances.ToArray());
        error = default;
        return true;
    }

}
