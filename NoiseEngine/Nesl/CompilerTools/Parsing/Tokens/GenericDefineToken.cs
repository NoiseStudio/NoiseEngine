using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

internal readonly record struct GenericDefineToken(
    IReadOnlyList<string> GenericParameters
) : IParserToken<GenericDefineToken> {

    public bool IsIgnored => GenericParameters.Count == 0;
    public int Priority => 0;

    public static bool Parse(
        TokenBuffer buffer, CompilationErrorMode errorMode, [NotNullWhen(true)] out GenericDefineToken result,
        out CompilationError error
    ) {
        if (!buffer.TryReadNext(TokenType.AngleBracketOpening, out Token bracket)) {
            buffer.Index--;
            result = new GenericDefineToken(Array.Empty<string>());
            error = default;
            return true;
        }
        if (bracket.Length < 2) {
            result = default;
            error = new CompilationError(bracket, CompilationErrorType.ExpectedClosingAngleBracket);
            return false;
        }

        List<string> parameters = new List<string>();
        while (true) {
            if (!buffer.TryReadNext(TokenType.Word, out Token parameter)) {
                result = default;
                error = new CompilationError(parameter, CompilationErrorType.ExpectedGenericParameter);
                return false;
            }
            parameters.Add(parameter.Value!);

            if (!buffer.TryReadNext(TokenType.Comma, out _)) {
                buffer.Index--;
                if (!buffer.TryReadNext(TokenType.AngleBracketClosing, out Token closingBracket)) {
                    result = default;
                    error = new CompilationError(closingBracket, CompilationErrorType.ExpectedClosingAngleBracket);
                    return false;
                }
                break;
            }
        }

        result = new GenericDefineToken(parameters.ToArray());
        error = default;
        return true;
    }

}
