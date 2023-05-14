using System;
using System.Diagnostics.CodeAnalysis;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

internal readonly record struct NameToken(CodePointer Pointer, string Name) : IParserToken<NameToken> {

    public bool IsIgnored => false;
    public int Priority => 0;

    public static bool Parse(
        TokenBuffer buffer, CompilationErrorMode errorMode, [NotNullWhen(true)] out NameToken result,
        out CompilationError error
    ) {
        if (!buffer.TryReadNext(TokenType.Word, out Token token)) {
            result = default;
            error = new CompilationError(token, CompilationErrorType.MissingName);
            return false;
        }

        result = new NameToken(token.Pointer, token.Value ?? throw new NotImplementedException());
        error = default;
        return true;
    }

}
