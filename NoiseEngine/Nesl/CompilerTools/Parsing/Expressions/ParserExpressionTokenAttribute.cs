using System;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;

[AttributeUsage(System.AttributeTargets.Method, AllowMultiple = true)]
internal class ParserExpressionTokenTypeAttribute : Attribute {

    public ParserTokenType TokenType { get; }
    public string Value { get; }

    public ParserExpressionTokenTypeAttribute(ParserTokenType tokenType, string value) {
        TokenType = tokenType;
        Value = value;
    }

}
