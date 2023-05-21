using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;

internal class FieldDeclaration : ParserExpressionContainer {

    public FieldDeclaration(Parser parser) : base(parser) {
    }

    [ParserExpression(ParserStep.TopLevel | ParserStep.Type)]
    [ParserExpressionParameter(ParserTokenType.AccessModifiers)]
    [ParserExpressionParameter(ParserTokenType.Modifiers)]
    [ParserExpressionParameter(ParserTokenType.TypeIdentifier)]
    [ParserExpressionParameter(ParserTokenType.Name)]
    [ParserExpressionTokenType(ParserTokenType.Semicolon)]
    public void Define(
        AccessModifiersToken accessModifiers, ModifiersToken modifiers, TypeIdentifierToken typeIdentifier,
        NameToken name
    ) {
        if (!Parser.TryDefineField(typeIdentifier, name.Name)) {
            Parser.Throw(new CompilationError(
                name.Pointer, CompilationErrorType.FieldOrPropertyAlreadyExists, name.Name
            ));
        }
    }

}
