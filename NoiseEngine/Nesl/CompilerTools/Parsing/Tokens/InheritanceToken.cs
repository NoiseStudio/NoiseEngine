using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

internal readonly record struct InheritanceToken(
    IReadOnlyList<TypeIdentifierToken> Inheritances
) : IParserToken<InheritanceToken> {

    public bool IsIgnored => Inheritances.Count == 0;
    public int Priority => 0;

    public static bool Parse(
        TokenBuffer buffer, CompilationErrorMode errorMode, [NotNullWhen(true)] out InheritanceToken result,
        out CompilationError error
    ) {
        int index = buffer.Index;
        if (!buffer.TryReadNext(TokenType.Colon)) {
            buffer.Index = index;
            result = new InheritanceToken(Array.Empty<TypeIdentifierToken>());
            error = default;
            return true;
        }

        List<TypeIdentifierToken> inheritances = new List<TypeIdentifierToken>();
        while (true) {
            if (!TypeIdentifierToken.Parse(buffer, errorMode, out TypeIdentifierToken inheritance, out error)) {
                result = default;
                return false;
            }
            inheritances.Add(inheritance);

            index = buffer.Index;
            if (!buffer.TryReadNext(TokenType.Comma)) {
                buffer.Index = index;
                break;
            }
        }

        result = new InheritanceToken(inheritances.ToArray());
        error = default;
        return true;
    }

}
