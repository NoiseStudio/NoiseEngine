using NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;
using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

namespace NoiseEngine.Nesl.CompilerTools.Parsing;

internal enum ParserTokenType {
    [ParserToken(typeof(ModifiersToken))]
    Modifiers,
    /*TypeIdentifier,
    Name,
    RoundBrackets,
    CurlyBrackets*/
}
