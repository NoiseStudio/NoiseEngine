using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;
using System.Linq;

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

        if (
            !Parser.Assembly.Types.Concat(Parser.Assembly.Dependencies.SelectMany(x => x.Types))
                .Any(x => x.Namespace == typeIdentifier.Identifier)
        ) {
            string ns = Parser.GetNamespaceFromFilePath(typeIdentifier.Pointer.Path);
            if (!NamespaceUtils.IsPartOf(typeIdentifier.Identifier, ns)) {
                Parser.Throw(new CompilationError(
                    typeIdentifier.Pointer, CompilationErrorType.UsingNotFound, typeIdentifier.Identifier
                ));
                return;
            }
        }

        if (!Parser.TryDefineUsing(typeIdentifier.Identifier)) {
            Parser.Throw(new CompilationError(
                typeIdentifier.Pointer, CompilationErrorType.UsingAlreadyExists, typeIdentifier.Identifier
            ));
        }
    }

}
