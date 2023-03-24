using System.Diagnostics.CodeAnalysis;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

internal readonly record struct TypeKindToken(NeslTypeKind TypeKind) : IParserToken<TypeKindToken> {

    public bool IsIgnored => false;
    public int Priority => 0;

    public static bool Parse(
        TokenBuffer buffer, CompilationErrorMode errorMode, [NotNullWhen(true)] out TypeKindToken result,
        out CompilationError error
    ) {
        if (!buffer.TryReadNext(TokenType.Word, out Token token)) {
            result = default;
            error = new CompilationError(token, CompilationErrorType.ExpectedTypeIdentifier);
            return false;
        }

        NeslTypeKind typeKind;
        switch (token.Value) {
            case "class":
                typeKind = NeslTypeKind.Class;
                break;
            case "struct":
                typeKind = NeslTypeKind.Struct;
                break;
            case "interface":
                typeKind = NeslTypeKind.Interface;
                break;
            default:
                result = default;
                error = new CompilationError(token, CompilationErrorType.InvalidTypeKind);
                return false;
        }

        result = new TypeKindToken(typeKind);
        error = default;
        return true;
    }

}
