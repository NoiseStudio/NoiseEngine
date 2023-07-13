using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;

internal class VariableAssignmentDeclaration : ParserExpressionContainer {

    public VariableAssignmentDeclaration(Parser parser) : base(parser) {
    }

    [ParserExpression(ParserStep.Method)]
    [ParserExpressionParameter(ParserTokenType.TypeIdentifier)]
    [ParserExpressionParameter(ParserTokenType.Name)]
    [ParserExpressionParameter(ParserTokenType.Operator)]
    [ParserExpressionParameter(ParserTokenType.Value)]
    [ParserExpressionTokenType(ParserTokenType.Semicolon)]
    public void Define(TypeIdentifierToken typeIdentifier, NameToken name, OperatorToken op, ValueToken value) {
        bool successful = true;
        if (!op.IsAssignment) {
            Parser.Throw(new CompilationError(op.Pointer, CompilationErrorType.ExpectedAssignmentOperator, op));
            successful = false;
        }

        VariableData? variable = new VariableDeclaration(Parser).DefineWorker(typeIdentifier, name);
        if (variable is null)
            return;

        new AssignmentDeclaration(Parser).AssignmentWorker(
            successful, op, value, (variable.Value.Type, variable.Value.Id, false, null, null)
        );
    }

}
