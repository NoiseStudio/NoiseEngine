using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;
using NoiseEngine.Nesl.CompilerTools.Parsing.Utils;
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
        AttributesToken attributes, AccessModifiersToken accessModifiers, ModifiersToken modifiers,
        TypeIdentifierToken typeIdentifier, NameToken name, CurlyBracketsToken curlyBrackets
    ) {
        name.AssertNameForFieldOrProperty(Parser);

        GetterSetterUtilsResult? result = GetterSetterUtils.FromCurlyBrackets(Parser, curlyBrackets, attributes);
        if (result is null)
            return;
        GetterSetterUtilsResult r = result.Value;

        if (!Parser.TryDefineProperty(new PropertyDefinitionData(
            modifiers.Modifiers, typeIdentifier, name, r.HasSetter, r.HasInitializer, r.Getter, r.GetterAttributes,
            r.Second, r.SecondAttributes
        ))) {
            Parser.Throw(new CompilationError(
                name.Pointer, CompilationErrorType.FieldOrPropertyOrIndexerAlreadyExists, name.Name
            ));
        }
    }

}
