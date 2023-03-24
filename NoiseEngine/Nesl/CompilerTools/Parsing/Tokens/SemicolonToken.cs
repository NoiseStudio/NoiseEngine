using System.Diagnostics.CodeAnalysis;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

internal readonly record struct SemicolonToken : IParserToken<SemicolonToken> {

    public bool IsIgnored => false;
    public int Priority => 0;

    public static bool Parse(
        TokenBuffer buffer, CompilationErrorMode errorMode, [NotNullWhen(true)] out SemicolonToken result,
        out CompilationError error
    ) {
        if (!buffer.TryReadNext(TokenType.Semicolon, out Token token)) {
            result = default;
            error = new CompilationError(token, CompilationErrorType.ExpectedSemicolon);
            return false;
        }

        result = new SemicolonToken();
        error = default;
        return true;
    }

}
