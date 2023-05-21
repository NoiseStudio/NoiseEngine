using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;

internal class ConstructorDeclaration : ParserExpressionContainer {

    public ConstructorDeclaration(Parser parser) : base(parser) {
    }

    [ParserExpression(ParserStep.Type)]
    [ParserExpressionParameter(ParserTokenType.Attributes)]
    [ParserExpressionParameter(ParserTokenType.AccessModifiers)]
    [ParserExpressionParameter(ParserTokenType.Name)]
    [ParserExpressionParameter(ParserTokenType.RoundBrackets)]
    [ParserExpressionParameter(ParserTokenType.CurlyBrackets)]
    public void Define(
        AttributesToken attributes, AccessModifiersToken accessModifiers, NameToken name, RoundBracketsToken parameters,
        CurlyBracketsToken codeBlock
    ) {
        Parser.DefineMethod(new MethodDefinitionData(
            null, name with { Name = NeslOperators.Constructor }, parameters.Buffer, codeBlock.Buffer,
            attributes.Compile(Parser, AttributeTargets.Method)
        ));
    }

}
