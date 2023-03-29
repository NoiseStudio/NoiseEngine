using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;
using NoiseEngine.Nesl.Emit;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;

internal class ReturnDeclaration : ParserExpressionContainer {

    public ReturnDeclaration(Parser parser) : base(parser) {
    }

    [ParserExpression(ParserStep.Method)]
    [ParserExpressionTokenType(ParserTokenType.Return)]
    [ParserExpressionTokenType(ParserTokenType.Semicolon)]
    public void Define() {
        Parser.CurrentMethod.IlGenerator.Emit(OpCode.Return);
    }

}
