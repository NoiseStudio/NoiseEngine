using NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;
using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

namespace NoiseEngine.Nesl.CompilerTools.Parsing;

internal enum ParserTokenType {
    [ParserToken(typeof(AccessModifiersToken))]
    AccessModifiers,
    [ParserToken(typeof(ModifiersToken))]
    Modifiers,
    [ParserToken(typeof(TypeIdentifierToken))]
    TypeIdentifier,
    [ParserToken(typeof(TypeKindToken))]
    TypeKind,
    [ParserToken(typeof(NameToken))]
    Name,
    [ParserToken(typeof(UsingToken))]
    Using,
    [ParserToken(typeof(ReturnToken))]
    Return,
    [ParserToken(typeof(ValueToken))]
    Value,
    [ParserToken(typeof(RoundBracketsToken))]
    RoundBrackets,
    [ParserToken(typeof(CurlyBracketsToken))]
    CurlyBrackets,
    [ParserToken(typeof(SemicolonToken))]
    Semicolon,
    [ParserToken(typeof(CommaToken))]
    Comma,
}
