using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;
using System;
using System.Collections.Generic;

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
        bool successful = buffer.TryReadNext(TokenType.Word, out Token word);

        if (!successful || word.Value! != "get") {
            Parser.Throw(new CompilationError(word, CompilationErrorType.ExpectedGetter));
            successful = false;
        }
        if (successful && !SemicolonToken.Parse(buffer, Parser.ErrorMode, out _, out CompilationError semicolonError)) {
            Parser.Throw(semicolonError);
            buffer.Index--;
        }

        bool hasSetter = false;
        bool hasInitializer = false;
        if (buffer.TryReadNext(TokenType.Word, out word)) {
            hasSetter = word.Value! == "set";
            hasInitializer = word.Value! == "init";

            if (!hasSetter && !hasInitializer) {
                Parser.Throw(new CompilationError(word, CompilationErrorType.ExpectedGetter));
            } else if (!SemicolonToken.Parse(buffer, Parser.ErrorMode, out _, out semicolonError)) {
                Parser.Throw(semicolonError);
                buffer.Index--;
            }
        }

        if (buffer.TryReadNext(out word))
            Parser.Throw(new CompilationError(word, CompilationErrorType.UnexpectedExpression));

        if (!successful)
            return;

        IReadOnlyList<NeslAttribute> attributes = attribute.Compile(Parser, AttributeTargets.Method);
        if (!Parser.TryDefineProperty(new PropertyDefinitionData(
            typeIdentifier, name, hasSetter, hasInitializer, null, attributes, null, attributes
        ))) {
            Parser.Throw(new CompilationError(
                name.Pointer, CompilationErrorType.FieldOrPropertyAlreadyExists, name.Name
            ));
        }
    }

}
