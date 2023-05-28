using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;

internal class AbstractMethodDeclaration : ParserExpressionContainer {

    public AbstractMethodDeclaration(Parser parser) : base(parser) {
    }

    [ParserExpression(ParserStep.Type)]
    [ParserExpressionParameter(ParserTokenType.Attributes)]
    [ParserExpressionParameter(ParserTokenType.AccessModifiers)]
    [ParserExpressionParameter(ParserTokenType.Modifiers)]
    [ParserExpressionParameter(ParserTokenType.TypeIdentifier)]
    [ParserExpressionParameter(ParserTokenType.Name)]
    [ParserExpressionParameter(ParserTokenType.RoundBrackets)]
    [ParserExpressionTokenType(ParserTokenType.Semicolon)]
    public void Define(
        AttributesToken attributes, AccessModifiersToken accessModifiers, ModifiersToken modifiers,
        TypeIdentifierToken typeIdentifier, NameToken name, RoundBracketsToken parameters
    ) {
        if (!Parser.CurrentType.IsInterface) {
            Parser.Throw(new CompilationError(
                name.Pointer, CompilationErrorType.AbstractMethodMustBeInInterface, name.Name
            ));
            return;
        }

        Parser.DefineMethod(new MethodDefinitionData(
            modifiers.Modifiers | NeslModifiers.Abstract, typeIdentifier, name, parameters.Buffer, null,
            attributes.Compile(Parser, AttributeTargets.Method)
        ));
    }

}
