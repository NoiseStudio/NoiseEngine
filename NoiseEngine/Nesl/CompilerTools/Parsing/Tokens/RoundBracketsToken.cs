using System.Diagnostics.CodeAnalysis;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

internal readonly record struct RoundBracketsToken(TokenBuffer Buffer) : IParserToken<RoundBracketsToken> {

    public bool IsIgnored => false;
    public int Priority => 0;

    public static bool Parse(
        TokenBuffer buffer, CompilationErrorMode errorMode, [NotNullWhen(true)] out RoundBracketsToken result,
        out CompilationError error
    ) {
        if (!buffer.TryReadNext(TokenType.RoundBracketOpening, out Token token)) {
            result = default;
            error = new CompilationError(token, CompilationErrorType.ExpectedOpeningRoundBracket);
            return false;
        }

        if (token.Length <= 1) {
            result = default;
            error = new CompilationError(token, CompilationErrorType.ExpectedClosingRoundBracket);
            return false;
        }

        result = new RoundBracketsToken(new TokenBuffer(buffer.Tokens.Slice(buffer.Index, token.Length - 2)));
        error = default;

        buffer.Index += token.Length - 1;
        return true;
    }

}
