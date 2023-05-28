using NoiseEngine.Nesl.CompilerTools.Parsing.Constructors;
using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;
using NoiseEngine.Nesl.Emit;
using System;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;

internal class ReturnValueDeclaration : ParserExpressionContainer {

    public ReturnValueDeclaration(Parser parser) : base(parser) {
    }

    [ParserExpression(ParserStep.Method)]
    [ParserExpressionTokenType(ParserTokenType.Return)]
    [ParserExpressionParameter(ParserTokenType.Value)]
    [ParserExpressionTokenType(ParserTokenType.Semicolon)]
    public void Define(ValueToken value) {
        ValueData valueData = ValueConstructor.Construct(value, Parser);
        if (Parser.CurrentMethod.ReturnType is not null)
            valueData = valueData.LoadConst(Parser, Parser.CurrentMethod.ReturnType);

        Parser.CurrentMethod.IlGenerator.Emit(OpCode.ReturnValue, valueData.Id);
    }

}
