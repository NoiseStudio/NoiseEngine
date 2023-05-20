using System.Diagnostics.CodeAnalysis;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

internal readonly record struct AccessModifiersToken(NeslAccessModifier Modifier) : IParserToken<AccessModifiersToken> {

    public bool IsIgnored => Modifier == NeslAccessModifier.Private;
    public int Priority => 0;

    public static bool Parse(
        TokenBuffer buffer, CompilationErrorMode errorMode, [NotNullWhen(true)] out AccessModifiersToken result,
        out CompilationError error
    ) {
        NeslAccessModifier modifier;
        if (buffer.TryReadNext(TokenType.Word, out Token token)) {
            switch (token.Value) {
                case "local":
                    modifier = NeslAccessModifier.Local;
                    break;
                case "internal":
                    modifier = NeslAccessModifier.Internal;
                    break;
                case "public":
                    modifier = NeslAccessModifier.Public;
                    break;
                default:
                    modifier = NeslAccessModifier.Private;
                    buffer.Index--;
                    break;
            }
        } else {
            modifier = NeslAccessModifier.Private;
            buffer.Index--;
        }

        result = new AccessModifiersToken(modifier);
        error = default;
        return true;
    }

}
