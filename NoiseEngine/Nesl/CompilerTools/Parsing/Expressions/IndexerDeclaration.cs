using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;
using NoiseEngine.Nesl.CompilerTools.Parsing.Utils;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;

internal class IndexerDeclaration : ParserExpressionContainer {

    public IndexerDeclaration(Parser parser) : base(parser) {
    }

    [ParserExpression(ParserStep.TopLevel | ParserStep.Type)]
    [ParserExpressionParameter(ParserTokenType.Attributes)]
    [ParserExpressionParameter(ParserTokenType.AccessModifiers)]
    [ParserExpressionParameter(ParserTokenType.Modifiers)]
    [ParserExpressionParameter(ParserTokenType.TypeIdentifier)]
    [ParserExpressionParameter(ParserTokenType.Name)]
    [ParserExpressionParameter(ParserTokenType.SquareBrackets)]
    [ParserExpressionParameter(ParserTokenType.CurlyBrackets)]
    public void Define(
        AttributesToken attributes, AccessModifiersToken accessModifiers, ModifiersToken modifiers,
        TypeIdentifierToken typeIdentifier, NameToken name, SquareBracketsToken squareBrackets, CurlyBracketsToken
        curlyBrackets
    ) {
        // Get index.
        TokenBuffer buffer = squareBrackets.Buffer;
        bool successful = true;
        if (!TypeIdentifierToken.Parse(
            buffer, Parser.ErrorMode, out TypeIdentifierToken indexType, out CompilationError error
        )) {
            Parser.Throw(error);
            successful = false;
        }

        if (!NameToken.Parse(buffer, Parser.ErrorMode, out NameToken indexName, out error)) {
            Parser.Throw(error);
            successful = false;
        }

        if (buffer.TryReadNext(out Token token))
            Parser.Throw(new CompilationError(token, CompilationErrorType.UnexpectedExpression));

        // Get body.
        GetterSetterUtilsResult? result = GetterSetterUtils.FromCurlyBrackets(Parser, curlyBrackets, attributes);
        if (!successful || result is null)
            return;
        GetterSetterUtilsResult r = result.Value;

        if (r.HasInitializer) {
            Parser.Throw(new CompilationError(
                curlyBrackets.Buffer.Tokens[0].Pointer, CompilationErrorType.InitializerForIndexerNotAllowed, "init"
            ));
            return;
        }

        // Define.
        if (!Parser.TryDefineIndexer(new IndexerDefinitionData(
            modifiers.Modifiers, typeIdentifier, name, indexType, indexName, r.Getter, r.GetterAttributes, r.HasSetter,
            r.Second, r.SecondAttributes
        ))) {
            Parser.Throw(new CompilationError(
                name.Pointer, CompilationErrorType.FieldOrPropertyOrIndexerAlreadyExists, name.Name
            ));
        }
    }

}
