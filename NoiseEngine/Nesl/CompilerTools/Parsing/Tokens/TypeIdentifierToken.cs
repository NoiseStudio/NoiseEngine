using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

internal readonly record struct TypeIdentifierToken(
    CodePointer Pointer, string Identifier, IReadOnlyList<TypeIdentifierToken> GenericTokens
) : IParserToken<TypeIdentifierToken> {

    public bool IsIgnored => false;
    public int Priority => 0;

    public static bool Parse(
        TokenBuffer buffer, CompilationErrorMode errorMode, [NotNullWhen(true)] out TypeIdentifierToken result,
        out CompilationError error
    ) {
        if (!buffer.TryReadNext(TokenType.Word, out Token token)) {
            result = default;
            error = new CompilationError(token, CompilationErrorType.ExpectedTypeIdentifier);
            return false;
        }

        CodePointer pointer = token.Pointer;

        StringBuilder builder = new StringBuilder(token.Value);
        int index = buffer.Index;

        while (buffer.TryReadNext(TokenType.Dot) && buffer.TryReadNext(TokenType.Word, out token)) {
            builder.Append('.').Append(token.Value);
            index = buffer.Index;
        }
        buffer.Index = index;

        if (buffer.TryReadNext(TokenType.AngleBracketOpening, out token)) {
            if (token.Length == 1) {
                buffer.TryReadNext(TokenType.None, out token);
                buffer.Index--;

                result = default;
                error = new CompilationError(token, CompilationErrorType.ExpectedClosingAngleBracket);
                return false;
            }

            List<TypeIdentifierToken> genericTokens = new List<TypeIdentifierToken>();
            do {
                if (!Parse(buffer, errorMode, out TypeIdentifierToken genericToken, out error)) {
                    result = default;
                    return false;
                }

                genericTokens.Add(genericToken);
            } while (buffer.TryReadNext(TokenType.Comma, out _));

            result = new TypeIdentifierToken(pointer, builder.ToString(), genericTokens.ToArray());
            buffer.Index = index + token.Length;
        } else {
            buffer.Index = index;
            result = new TypeIdentifierToken(pointer, builder.ToString(), Array.Empty<TypeIdentifierToken>());
        }

        error = default;
        return true;
    }

    public NeslType? GetTypeFromAssembly(
        NeslAssembly assembly, IEnumerable<string> usings, out IReadOnlyList<TypeIdentifierToken> genericTokens
    ) {
        NeslType? type = assembly.GetType(Identifier);
        genericTokens = GenericTokens;
        if (type is null) {
            foreach (string u in usings) {
                type = assembly.GetType(u + "." + Identifier);
                if (type is not null)
                    return type;
            }

            if (type is null) {
                if (TryGetBuiltInIdentifier(out TypeIdentifierToken ti)) {
                    type = assembly.GetType(ti.Identifier);
                    if (type is null)
                        return null;
                    genericTokens = ti.GenericTokens;
                } else {
                    return null;
                }
            }
        }

        return type;
    }

    private bool TryGetBuiltInIdentifier(out TypeIdentifierToken typeIdentifier) {
        if (GenericTokens.Count != 0) {
            typeIdentifier = default;
            return false;
        }

        switch (Identifier) {
            case "f32":
                typeIdentifier = GetSystemType("Float32");
                break;
            case "f32v2":
                typeIdentifier = GetSystemGenericType("Vector2", "Float32");
                break;
            case "f32v3":
                typeIdentifier = GetSystemGenericType("Vector3", "Float32");
                break;
            case "f32v4":
                typeIdentifier = GetSystemGenericType("Vector4", "Float32");
                break;
            default:
                typeIdentifier = default;
                return false;
        }

        return true;
    }

    private TypeIdentifierToken GetSystemType(string name) {
        return new TypeIdentifierToken(Pointer, $"System.{name}", Array.Empty<TypeIdentifierToken>());
    }

    private TypeIdentifierToken GetSystemGenericType(string name, string nameGeneric) {
        return new TypeIdentifierToken(
            Pointer, $"System.{name}`1", new TypeIdentifierToken[] { GetSystemType(nameGeneric) }
        );
    }

}
