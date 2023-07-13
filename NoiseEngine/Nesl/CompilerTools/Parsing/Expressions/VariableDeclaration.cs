using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;

internal class VariableDeclaration : ParserExpressionContainer {

    public VariableDeclaration(Parser parser) : base(parser) {
    }

    [ParserExpression(ParserStep.Method)]
    [ParserExpressionParameter(ParserTokenType.TypeIdentifier)]
    [ParserExpressionParameter(ParserTokenType.Name)]
    [ParserExpressionTokenType(ParserTokenType.Semicolon)]
    public void Define(TypeIdentifierToken typeIdentifier, NameToken name) {
        DefineWorker(typeIdentifier, name);
    }

    public VariableData? DefineWorker(TypeIdentifierToken typeIdentifier, NameToken name) {
        if (!Parser.TryGetType(typeIdentifier, out NeslType? type))
            return null;
        return Parser.DefineVariable(type, name);
    }

}
