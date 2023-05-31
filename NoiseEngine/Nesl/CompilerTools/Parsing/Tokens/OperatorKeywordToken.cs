using System.Diagnostics.CodeAnalysis;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

internal readonly record struct OperatorKeywordToken : IParserToken<OperatorKeywordToken> {

    public bool IsIgnored => false;
    public int Priority => 1;

    public static bool Parse(
        TokenBuffer buffer, CompilationErrorMode errorMode, [NotNullWhen(true)] out OperatorKeywordToken result,
        out CompilationError error
    ) {
        if (!buffer.TryReadNext(TokenType.Word, out Token token) || token.Value != "operator") {
            result = default;
            error = new CompilationError(token, CompilationErrorType.ExpectedOperatorKeyword);
            return false;
        }

        result = new OperatorKeywordToken();
        error = default;
        return true;
    }

}
