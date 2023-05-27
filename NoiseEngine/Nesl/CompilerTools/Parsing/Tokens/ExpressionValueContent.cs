namespace NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

internal readonly record struct ExpressionValueContent(
    bool IsNew,
    TypeIdentifierToken? Identifier,
    RoundBracketsToken? RoundBrackets,
    CurlyBracketsToken? CurlyBrackets,
    ValueToken? Indexer
);
