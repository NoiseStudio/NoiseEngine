using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;

internal class MethodDeclaration : ParserExpressionContainer {

    public MethodDeclaration(Parser parser) : base(parser) {
    }

    [ParserExpression(ParserStep.TopLevel | ParserStep.Type)]
    [ParserExpressionParameter(ParserTokenType.Attributes)]
    [ParserExpressionParameter(ParserTokenType.AccessModifiers)]
    [ParserExpressionParameter(ParserTokenType.Modifiers)]
    [ParserExpressionParameter(ParserTokenType.TypeIdentifier)]
    [ParserExpressionParameter(ParserTokenType.Name)]
    [ParserExpressionParameter(ParserTokenType.RoundBrackets)]
    [ParserExpressionParameter(ParserTokenType.Constraints)]
    [ParserExpressionParameter(ParserTokenType.CurlyBrackets)]
    public void Define(
        AttributesToken attributes, AccessModifiersToken accessModifiers, ModifiersToken modifiers,
        TypeIdentifierToken typeIdentifier, NameToken name, RoundBracketsToken parameters, ConstraintsToken constraints,
        CurlyBracketsToken codeBlock
    ) {
        if (modifiers.Modifiers.HasFlag(NeslModifiers.Uniform)) {
            Parser.Throw(new CompilationError(
                name.Pointer, CompilationErrorType.UniformMethodNotAllowed, modifiers.Modifiers
            ));
        }

        Parser.DefineMethod(new MethodDefinitionData(
            modifiers.Modifiers, typeIdentifier, name, parameters.Buffer, constraints.Constraints, codeBlock.Buffer,
            attributes.Compile(Parser, AttributeTargets.Method)
        ));
    }

}
