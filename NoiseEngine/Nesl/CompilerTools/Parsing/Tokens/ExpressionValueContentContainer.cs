using System.Collections.Generic;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

internal record ExpressionValueContentContainer(
    ValueToken? BracketToken, IReadOnlyList<ExpressionValueContent> Expressions
) : IValueContent {

    public CodePointer Pointer => Expressions[0].Identifier!.Value.Pointer;

}
