using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

internal readonly record struct ConstraintsToken(
    IReadOnlyList<ConstraintToken> Constraints
) : IParserToken<ConstraintsToken> {

    public bool IsIgnored => Constraints.Count == 0;
    public int Priority => 0;

    public static bool Parse(
        TokenBuffer buffer, CompilationErrorMode errorMode, [NotNullWhen(true)] out ConstraintsToken result,
        out CompilationError error
    ) {
        int index = buffer.Index;
        List<ConstraintToken> constraints = new List<ConstraintToken>();
        while (ConstraintToken.Parse(buffer, errorMode, out ConstraintToken constraint, out _)) {
            constraints.Add(constraint);
            index = buffer.Index;
        }

        buffer.Index = index;
        result = new ConstraintsToken(constraints.ToArray());
        error = default;
        return true;
    }

}
