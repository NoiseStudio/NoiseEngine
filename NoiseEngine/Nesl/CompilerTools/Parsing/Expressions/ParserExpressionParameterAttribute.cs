using System;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;

[AttributeUsage(System.AttributeTargets.Method, AllowMultiple = true)]
internal class ParserExpressionParameterAttribute : Attribute {

    public ParserTokenType TokenType { get; }

    public ParserExpressionParameterAttribute(ParserTokenType tokenType) {
        TokenType = tokenType;
    }

}
