using System.Diagnostics.CodeAnalysis;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

internal readonly record struct CommaToken : IParserToken<CommaToken> {

    public bool IsIgnored => false;
    public int Priority => 0;

    public static bool Parse(
        TokenBuffer buffer, CompilationErrorMode errorMode, [NotNullWhen(true)] out CommaToken result,
        out CompilationError error
    ) {
        if (!buffer.TryReadNext(TokenType.Comma, out Token token)) {
            result = default;
            error = new CompilationError(token, CompilationErrorType.ExpectedComma);
            return false;
        }

        result = new CommaToken();
        error = default;
        return true;
    }

}
