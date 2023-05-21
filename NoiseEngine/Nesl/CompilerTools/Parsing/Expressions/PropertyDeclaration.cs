using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;
using System;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;

internal class PropertyDeclaration : ParserExpressionContainer {

    public PropertyDeclaration(Parser parser) : base(parser) {
    }

    [ParserExpression(ParserStep.TopLevel | ParserStep.Type)]
    [ParserExpressionParameter(ParserTokenType.Attributes)]
    [ParserExpressionParameter(ParserTokenType.AccessModifiers)]
    [ParserExpressionParameter(ParserTokenType.Modifiers)]
    [ParserExpressionParameter(ParserTokenType.TypeIdentifier)]
    [ParserExpressionParameter(ParserTokenType.Name)]
    [ParserExpressionParameter(ParserTokenType.CurlyBrackets)]
    public void Define(
        AttributesToken attribute, AccessModifiersToken accessModifiers, ModifiersToken modifiers,
        TypeIdentifierToken typeIdentifier, NameToken name, CurlyBracketsToken curlyBrackets
    ) {
        TokenBuffer buffer = curlyBrackets.Buffer;
        bool hasWord = buffer.TryReadNext(TokenType.Word, out Token word);
        if (!hasWord || word.Value! != "get")
            Parser.Throw(new CompilationError(word, CompilationErrorType.ExpectedGetter));
        if (hasWord && !SemicolonToken.Parse(buffer, Parser.ErrorMode, out _, out CompilationError semicolonError))
            Parser.Throw(semicolonError);

        if (!hasWord)
            return;

        if (!Parser.TryDefineProperty(new PropertyDefinitionData(
            typeIdentifier, name, false, false, null, attribute.Compile(Parser, AttributeTargets.Method), null,
            Array.Empty<NeslAttribute>()
        ))) {
            Parser.Throw(new CompilationError(
                name.Pointer, CompilationErrorType.FieldOrPropertyAlreadyExists, name.Name
            ));
        }
    }

}
