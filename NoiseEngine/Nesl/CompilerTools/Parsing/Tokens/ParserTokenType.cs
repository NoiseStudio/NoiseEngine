using NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;
using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

namespace NoiseEngine.Nesl.CompilerTools.Parsing;

internal enum ParserTokenType {
    [ParserToken(typeof(AttributesToken))]
    Attributes,
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
    [ParserToken(typeof(OperatorToken))]
    Operator,
    [ParserToken(typeof(RoundBracketsToken))]
    RoundBrackets,
    [ParserToken(typeof(CurlyBracketsToken))]
    CurlyBrackets,
    [ParserToken(typeof(SquareBracketsToken))]
    SquareBrackets,
    [ParserToken(typeof(GenericDefineToken))]
    GenericDefine,
    [ParserToken(typeof(InheritanceToken))]
    Inheritance,
    [ParserToken(typeof(ConstraintsToken))]
    Constraints,
    [ParserToken(typeof(SemicolonToken))]
    Semicolon,
    [ParserToken(typeof(CommaToken))]
    Comma,
}
