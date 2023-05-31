using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;
using System;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;

internal class OperatorDeclaration : ParserExpressionContainer {

    public OperatorDeclaration(Parser parser) : base(parser) {
    }

    [ParserExpression(ParserStep.TopLevel | ParserStep.Type)]
    [ParserExpressionParameter(ParserTokenType.Attributes)]
    [ParserExpressionParameter(ParserTokenType.AccessModifiers)]
    [ParserExpressionParameter(ParserTokenType.Modifiers)]
    [ParserExpressionParameter(ParserTokenType.TypeIdentifier)]
    [ParserExpressionTokenType(ParserTokenType.OperatorKeyword)]
    [ParserExpressionParameter(ParserTokenType.Operator)]
    [ParserExpressionParameter(ParserTokenType.RoundBrackets)]
    [ParserExpressionParameter(ParserTokenType.CurlyBrackets)]
    public void Define(
        AttributesToken attributes, AccessModifiersToken accessModifiers, ModifiersToken modifiers,
        TypeIdentifierToken typeIdentifier, OperatorToken op, RoundBracketsToken parameters,
        CurlyBracketsToken codeBlock
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
            modifiers.Modifiers | NeslModifiers.Static, typeIdentifier, name, parameters.Buffer, codeBlock.Buffer,
            attributes.Compile(Parser, AttributeTargets.Method)
        ));
    }

}
