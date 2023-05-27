using System.Diagnostics.CodeAnalysis;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

internal readonly record struct SquareBracketsToken(TokenBuffer Buffer) : IParserToken<SquareBracketsToken> {

    public bool IsIgnored => false;
    public int Priority => 0;

    public static bool Parse(
        TokenBuffer buffer, CompilationErrorMode errorMode, [NotNullWhen(true)] out SquareBracketsToken result,
        out CompilationError error
    ) {
        if (!buffer.TryReadNext(TokenType.SquareBracketOpening, out Token token)) {
            result = default;
            error = new CompilationError(token, CompilationErrorType.ExpectedOpeningSquareBracket);
            return false;
        }

        if (token.Length <= 1) {
            result = default;
            error = new CompilationError(token, CompilationErrorType.ExpectedClosingSquareBracket);
            return false;
        }

        result = new SquareBracketsToken(new TokenBuffer(buffer.Tokens.Slice(buffer.Index, token.Length - 2)));
        error = default;

        buffer.Index += token.Length - 1;
        return true;
    }

}
