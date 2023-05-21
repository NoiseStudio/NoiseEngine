using System.Diagnostics.CodeAnalysis;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

internal readonly record struct CurlyBracketsToken(TokenBuffer Buffer) : IParserToken<CurlyBracketsToken> {

    public bool IsIgnored => false;
    public int Priority => 0;

    public static bool Parse(
        TokenBuffer buffer, CompilationErrorMode errorMode, [NotNullWhen(true)] out CurlyBracketsToken result,
        out CompilationError error
    ) {
        if (!buffer.TryReadNext(TokenType.CurlyBracketOpening, out Token token)) {
            result = default;
            error = new CompilationError(token, CompilationErrorType.ExpectedOpeningCurlyBracket);
            return false;
        }

        if (token.Length <= 1) {
            result = default;
            error = new CompilationError(token, CompilationErrorType.ExpectedClosingCurlyBracket);
            return false;
        }

        result = new CurlyBracketsToken(new TokenBuffer(buffer.Tokens.Slice(buffer.Index, token.Length - 2)));
        error = default;

        buffer.Index += token.Length - 1;
        return true;
    }

}
