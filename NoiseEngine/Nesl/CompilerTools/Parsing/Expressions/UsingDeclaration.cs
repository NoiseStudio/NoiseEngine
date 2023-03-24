using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;

internal class UsingDeclaration : ParserExpressionContainer {

    public UsingDeclaration(Parser parser) : base(parser) {
    }

    [ParserExpression(ParserStep.TopLevel)]
    [ParserExpressionTokenType(ParserTokenType.Using)]
    [ParserExpressionParameter(ParserTokenType.TypeIdentifier)]
    [ParserExpressionTokenType(ParserTokenType.Semicolon)]
    public void Define(TypeIdentifierToken typeIdentifier) {
        if (typeIdentifier.GenericTokens.Count != 0) {
            Parser.Throw(new CompilationError(
                typeIdentifier.Pointer, CompilationErrorType.UsingGenericNotAllowed, typeIdentifier.Identifier
            ));
        }

        if (!Parser.TryDefineUsing(typeIdentifier.Identifier)) {
            Parser.Throw(new CompilationError(
                typeIdentifier.Pointer, CompilationErrorType.UsingAlreadyExists, typeIdentifier.Identifier
            ));
        }
    }

}
