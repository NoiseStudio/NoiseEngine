using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

internal readonly record struct InheritancesToken(
    IReadOnlyList<InheritanceToken> Inheritances
) : IParserToken<InheritancesToken> {

    public bool IsIgnored => Inheritances.Count == 0;
    public int Priority => 0;

    public static bool Parse(
        TokenBuffer buffer, CompilationErrorMode errorMode, [NotNullWhen(true)] out InheritancesToken result,
        out CompilationError error
    ) {
        int index = buffer.Index;
        if (!buffer.TryReadNext(TokenType.Colon)) {
            buffer.Index = index;
            result = new InheritancesToken(Array.Empty<InheritanceToken>());
            error = default;
            return true;
        }

        List<InheritanceToken> inheritances = new List<InheritanceToken>();
        while (true) {
            if (!InheritanceToken.Parse(buffer, errorMode, out InheritanceToken inheritance, out error)) {
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

        result = new InheritancesToken(inheritances.ToArray());
        error = default;
        return true;
    }

}
