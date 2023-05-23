using NoiseEngine.Nesl.CompilerTools.Parsing.Constructors;
using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;
using NoiseEngine.Nesl.Emit;
using System;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;

internal class AssignmentDeclaration : ParserExpressionContainer {

    public AssignmentDeclaration(Parser parser) : base(parser) {
    }

    [ParserExpression(ParserStep.Method)]
    [ParserExpressionParameter(ParserTokenType.TypeIdentifier)]
    [ParserExpressionParameter(ParserTokenType.Operator)]
    [ParserExpressionParameter(ParserTokenType.Value)]
    [ParserExpressionTokenType(ParserTokenType.Semicolon)]
    public void Define(TypeIdentifierToken identifier, OperatorToken op, ValueToken value) {
        bool successful = true;
        if (identifier.GenericTokens.Count != 0) {
            Parser.Throw(new CompilationError(
                identifier.GenericTokens[0].Pointer, CompilationErrorType.AssigmentGenericNotAllowed, identifier
            ));
            successful = false;
        }
        if (!op.IsAssigment) {
            Parser.Throw(new CompilationError(op.Pointer, CompilationErrorType.ExpectedAssigmentOperator, op));
            successful = false;
        }

        int index = identifier.Identifier.IndexOf('.');
        if (index != -1)
            throw new NotImplementedException();

        VariableData? variable = Parser.GetVariable(identifier.Identifier);
        bool isField = false;

        // Check fields.
        if (variable is null) {
            uint i = 0;
            foreach (NeslField field in Parser.CurrentMethod.Type.Fields) {
                if (field.Name == identifier.Identifier) {
                    variable = new VariableData(field.FieldType, field.Name, i);
                    isField = true;
                    break;
                }
                i++;
            }
        }

        // TODO: Add properties.

        if (variable is null) {
            Parser.Throw(new CompilationError(
                identifier.Pointer, CompilationErrorType.VariableOrFieldOrPropertyNotFound, identifier.Identifier
            ));
            successful = false;
        }

        ValueData valueData = ValueConstructor.Construct(value, Parser);
        if (!successful || valueData.IsInvalid)
            return;
        valueData = valueData.LoadConst(Parser, variable!.Value.Type);

        IlGenerator il = Parser.CurrentMethod.IlGenerator;
        if (!isField)
            il.Emit(OpCode.Load, variable!.Value.Id, valueData.Id);
        else
            il.Emit(OpCode.SetField, Parser.InstanceVariableId, variable!.Value.Id, valueData.Id);
    }

}
