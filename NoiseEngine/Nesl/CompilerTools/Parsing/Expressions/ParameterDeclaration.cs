using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;

internal class ParameterDeclaration : ParserExpressionContainer {

    public ParameterDeclaration(Parser parser) : base(parser) {
    }

    [ParserExpression(ParserStep.Parameters)]
    [ParserExpressionParameter(ParserTokenType.TypeIdentifier)]
    [ParserExpressionParameter(ParserTokenType.Name)]
    [ParserExpressionTokenType(ParserTokenType.Comma)]
    public void Define(TypeIdentifierToken typeIdentifier, NameToken name) {
        ((ParameterParser)Parser).TryDefineParameter(typeIdentifier, name);
    }

}
