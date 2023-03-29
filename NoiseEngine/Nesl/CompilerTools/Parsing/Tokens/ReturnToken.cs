using System.Diagnostics.CodeAnalysis;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

internal readonly record struct ReturnToken : IParserToken<ReturnToken> {

    public bool IsIgnored => false;
    public int Priority => 1;

    public static bool Parse(
        TokenBuffer buffer, CompilationErrorMode errorMode, [NotNullWhen(true)] out ReturnToken result,
        out CompilationError error
    ) {
        if (!buffer.TryReadNext(TokenType.Word, out Token token) || token.Value != "return") {
            result = default;
            error = new CompilationError(token, CompilationErrorType.ExpectedReturn);
            return false;
        }

        result = new ReturnToken();
        error = default;
        return true;
    }

}
