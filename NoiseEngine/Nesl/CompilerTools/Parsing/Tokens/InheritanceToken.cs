using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

internal readonly record struct InheritanceToken(
    TypeIdentifierToken Inheritance, IReadOnlyList<InheritanceConstraintToken> Constraints
) : IParserToken<InheritanceToken> {

    public bool IsIgnored => false;
    public int Priority => 0;

    public static bool Parse(
        TokenBuffer buffer, CompilationErrorMode errorMode, [NotNullWhen(true)] out InheritanceToken result,
        out CompilationError error
    ) {
        if (!TypeIdentifierToken.Parse(buffer, errorMode, out TypeIdentifierToken inheritance, out error)) {
            result = default;
            return false;
        }

        int index = buffer.Index;
        List<InheritanceConstraintToken> constraints = new List<InheritanceConstraintToken>();
        while (InheritanceConstraintToken.Parse(
            buffer, errorMode, out InheritanceConstraintToken constraint, out error
        )) {
            constraints.Add(constraint);
            index = buffer.Index;
        }

        if (error.Type != CompilationErrorType.ExpectedForKeyword) {
            result = default;
            return false;
        }
        buffer.Index = index;

        result = new InheritanceToken(inheritance, constraints.ToArray());
        error = default;
        return true;
    }

}
