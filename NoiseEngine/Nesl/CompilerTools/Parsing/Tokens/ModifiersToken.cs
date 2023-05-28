using System.Diagnostics.CodeAnalysis;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

internal readonly record struct ModifiersToken(NeslModifiers Modifiers) : IParserToken<ModifiersToken> {

    public bool IsIgnored => Modifiers == NeslModifiers.None;
    public int Priority => 0;

    public static bool Parse(
        TokenBuffer buffer, CompilationErrorMode errorMode, [NotNullWhen(true)] out ModifiersToken result,
        out CompilationError error
    ) {
        NeslModifiers modifier;
        if (buffer.TryReadNext(TokenType.Word, out Token token)) {
            switch (token.Value) {
                case "static":
                    modifier = NeslModifiers.Static;
                    break;
                case "uniform":
                    modifier = NeslModifiers.Uniform;
                    break;
                default:
                    modifier = NeslModifiers.None;
                    buffer.Index--;
                    break;
            }
        } else {
            modifier = NeslModifiers.None;
            buffer.Index--;
        }

        result = new ModifiersToken(modifier);
        error = default;
        return true;
    }

}
