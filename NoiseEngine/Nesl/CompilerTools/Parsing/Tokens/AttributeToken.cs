using System.Diagnostics.CodeAnalysis;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

internal readonly record struct AttributeToken(
    CodePointer Pointer, TypeIdentifierToken Identifier, TokenBuffer? Parameters
) : IParserToken<AttributeToken> {

    public bool IsIgnored => false;
    public int Priority => 0;

    public static bool Parse(
        TokenBuffer buffer, CompilationErrorMode errorMode, [NotNullWhen(true)] out AttributeToken result,
        out CompilationError error
    ) {
        if (!buffer.TryReadNext(TokenType.SquareBracketOpening, out Token token)) {
            result = default;
            error = new CompilationError(token, CompilationErrorType.ExpectedOpeningSquareBracket);
            return false;
        }
        if (token.Length < 2) {
            result = default;
            error = new CompilationError(token, CompilationErrorType.ExpectedClosingSquareBracket);
            return false;
        }
        int finalIndex = buffer.Index - 1 + token.Length;
        CodePointer pointer = token.Pointer;

        if (!TypeIdentifierToken.Parse(buffer, errorMode, out TypeIdentifierToken typeIdentifier, out error)) {
            buffer.Index = finalIndex;
            result = default;
            return false;
        }
        if (typeIdentifier.GenericTokens.Count > 0) {
            buffer.Index = finalIndex;
            result = default;
            error = new CompilationError(
                typeIdentifier.Pointer, CompilationErrorType.AttributeGenericNotAllowed, typeIdentifier.Identifier
            );
            return false;
        }

        // Parameters.
        TokenBuffer? parametersBuffer;
        int index = buffer.Index;
        if (RoundBracketsToken.Parse(buffer, errorMode, out RoundBracketsToken parameters, out _)) {
            parametersBuffer = parameters.Buffer;
        } else {
            buffer.Index = index;
            parametersBuffer = null;
        }

        if (buffer.Index + 1 != finalIndex) {
            result = default;
            error = new CompilationError(token, CompilationErrorType.ExpectedClosingSquareBracket);
            return false;
        }

        buffer.Index = finalIndex;
        result = new AttributeToken(pointer, typeIdentifier, parametersBuffer);
        error = default;
        return true;
    }

}
