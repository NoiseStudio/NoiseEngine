using System.Diagnostics.CodeAnalysis;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

internal readonly record struct ModifiersToken(NeslModifier Modifiers) : IParserToken<ModifiersToken> {

    public bool IsIgnored => Modifiers == NeslModifier.None;
    public int Priority => 0;

    public static bool Parse(
        TokenBuffer buffer, CompilationErrorMode errorMode, [NotNullWhen(true)] out ModifiersToken result,
        out CompilationError error
    ) {
        NeslModifier modifier;
        if (buffer.TryReadNext(TokenType.Word, out Token token)) {
            switch (token.Value) {
                case "static":
                    modifier = NeslModifier.Static;
                    break;
                case "uniform":
                    modifier = NeslModifier.Uniform;
                    break;
                default:
                    modifier = NeslModifier.None;
                    buffer.Index--;
                    break;
            }
        } else {
            modifier = NeslModifier.None;
            buffer.Index--;
        }

        result = new ModifiersToken(modifier);
        error = default;
        return true;
    }

}
