using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;
using System;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;

internal class MethodDeclaration : ParserExpressionContainer {

    public MethodDeclaration(Parser parser) : base(parser) {
    }

    [ParserExpression(ParserStep.TopLevel | ParserStep.Type)]
    [ParserExpressionParameter(ParserTokenType.Modifiers)]
    //[ParserExpressionParameter(ParserTokenType.TypeIdentifier)]
    //[ParserExpressionParameter(ParserTokenType.Name)]
    //[ParserExpressionParameter(ParserTokenType.RoundBrackets)]
    //[ParserExpressionParameter(ParserTokenType.CurlyBrackets)]
    public void Define(ModifiersToken modifiers) {
        //throw new NotImplementedException();
    }

}
