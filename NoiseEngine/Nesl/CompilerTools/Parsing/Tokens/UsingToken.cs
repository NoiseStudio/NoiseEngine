using System.Diagnostics.CodeAnalysis;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

internal readonly record struct UsingToken : IParserToken<UsingToken> {

    public bool IsIgnored => false;
    public int Priority => 1;

    public static bool Parse(
        TokenBuffer buffer, CompilationErrorMode errorMode, [NotNullWhen(true)] out UsingToken result,
        out CompilationError error
    ) {
        if (!buffer.TryReadNext(TokenType.Word, out Token token) || token.Value != "using") {
            result = default;
            error = new CompilationError(token, CompilationErrorType.ExpectedUsing);
            return false;
        }

        result = new UsingToken();
        error = default;
        return true;
    }

}
