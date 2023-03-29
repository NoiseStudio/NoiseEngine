using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;

internal class ReturnValueDeclaration : ParserExpressionContainer {

    public ReturnValueDeclaration(Parser parser) : base(parser) {
    }

    [ParserExpression(ParserStep.Method)]
    [ParserExpressionTokenType(ParserTokenType.Return)]
    [ParserExpressionParameter(ParserTokenType.Value)]
    [ParserExpressionTokenType(ParserTokenType.Semicolon)]
    public void Define(ValueToken value) {
    }

}
