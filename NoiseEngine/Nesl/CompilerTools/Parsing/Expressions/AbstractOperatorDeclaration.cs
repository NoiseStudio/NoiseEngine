using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;
using System;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;

internal class AbstractOperatorDeclaration : ParserExpressionContainer {

    public AbstractOperatorDeclaration(Parser parser) : base(parser) {
    }

    [ParserExpression(ParserStep.Type)]
    [ParserExpressionParameter(ParserTokenType.Attributes)]
    [ParserExpressionParameter(ParserTokenType.AccessModifiers)]
    [ParserExpressionParameter(ParserTokenType.Modifiers)]
    [ParserExpressionParameter(ParserTokenType.TypeIdentifier)]
    [ParserExpressionTokenType(ParserTokenType.OperatorKeyword)]
    [ParserExpressionParameter(ParserTokenType.Operator)]
    [ParserExpressionParameter(ParserTokenType.RoundBrackets)]
    [ParserExpressionTokenType(ParserTokenType.Semicolon)]
    public void Define(
        AttributesToken attributes, AccessModifiersToken accessModifiers, ModifiersToken modifiers,
        TypeIdentifierToken typeIdentifier, OperatorToken op, RoundBracketsToken parameters
    ) {
        if (!modifiers.Modifiers.HasFlag(NeslModifiers.Static)) {
            Parser.Throw(new CompilationError(
                op.Pointer, CompilationErrorType.OperatorDeclarationMustBeStatic, modifiers.Modifiers
            ));
        }
        if (modifiers.Modifiers.HasFlag(NeslModifiers.Uniform)) {
            Parser.Throw(new CompilationError(
                op.Pointer, CompilationErrorType.UniformMethodNotAllowed, modifiers.Modifiers
            ));
        }
        if (!Parser.CurrentType.IsInterface) {
            Parser.Throw(new CompilationError(
                op.Pointer, CompilationErrorType.AbstractMethodMustBeInInterface, op
            ));
            return;
        }

        NameToken name;
        if (op.IsAssignment) {
            Parser.Throw(new CompilationError(
                op.Pointer, CompilationErrorType.AssignmentOperatorDeclarationNotAllowed, op
            ));
            name = new NameToken(op.Pointer, $"{NeslOperators.Operator}{Random.Shared.NextInt64()}");
        } else {
            name = new NameToken(op.Pointer, $"{NeslOperators.Operator}{op.Type}");
        }

        Parser.DefineMethod(new MethodDefinitionData(
            modifiers.Modifiers | NeslModifiers.Static | NeslModifiers.Abstract, typeIdentifier, name,
            parameters.Buffer, null, attributes.Compile(Parser, AttributeTargets.Method)
        ));
    }

}
