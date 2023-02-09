using System.Diagnostics.CodeAnalysis;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

internal readonly record struct ModifiersToken(NeslModifier Modifier) : IParserToken<ModifiersToken> {

    public static bool Parse(
        TokenBuffer buffer, CompilationErrorMode errorMode, [NotNullWhen(true)] out ModifiersToken result,
        out CompilationError error
    ) {
        if (!buffer.TryReadNext(TokenType.Word, out Token token)) {
            result = default;
            error = new CompilationError(token, CompilationErrorType.MissingModifiers);
            return false;
        }

        NeslModifier modifier;
        switch (token.Value) {
            case "static":
                modifier = NeslModifier.Static;
                break;
            case "uniform":
                modifier = NeslModifier.Uniform;
                break;
            default:
                result = default;
                error = new CompilationError(token, CompilationErrorType.MissingModifiers);
                return false;
        }

        result = new ModifiersToken(modifier);
        error = default;
        return true;
    }

}
