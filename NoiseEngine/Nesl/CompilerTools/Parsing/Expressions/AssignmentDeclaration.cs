using NoiseEngine.Nesl.CompilerTools.Parsing.Constructors;
using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;
using NoiseEngine.Nesl.Emit;
using System;
using System.Diagnostics;
using System.Linq;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;

internal class AssignmentDeclaration : ParserExpressionContainer {

    public AssignmentDeclaration(Parser parser) : base(parser) {
    }

    [ParserExpression(ParserStep.Method)]
    [ParserExpressionParameter(ParserTokenType.Value)]
    [ParserExpressionParameter(ParserTokenType.Operator)]
    [ParserExpressionParameter(ParserTokenType.Value)]
    [ParserExpressionTokenType(ParserTokenType.Semicolon)]
    public void Define(ValueToken assignment, OperatorToken op, ValueToken value) {
        bool successful = true;
        if (!op.IsAssignment) {
            Parser.Throw(new CompilationError(op.Pointer, CompilationErrorType.ExpectedAssignmentOperator, op));
            successful = false;
        }

        (NeslType type, uint id, bool isField, NeslMethod? indexerMethod, ValueToken? indexer)? v =
            GetVariable(assignment);

        ValueData valueData = ValueConstructor.Construct(value, Parser);

        ValueData? indexerData = null;
        NeslMethod? indexerMethod = null;
        if (v.HasValue && v.Value.indexer is not null) {
            indexerData = ValueConstructor.Construct(v.Value.indexer, Parser);
            indexerMethod = v.Value.indexerMethod;

            if (indexerMethod is null) {
                indexerMethod = v.Value.type.GetMethod(NeslOperators.IndexerSet);
                if (indexerMethod is null) {
                    Parser.Throw(new CompilationError(op.Pointer, CompilationErrorType.IndexerNotFound, "this"));
                    successful = false;
                }
            }
        }

        if (!successful || !v.HasValue || valueData.IsInvalid || (indexerData.HasValue && indexerData.Value.IsInvalid))
            return;

        IlGenerator il = Parser.CurrentMethod.IlGenerator;
        if (indexerData.HasValue) {
            if (indexerMethod is null)
                throw new UnreachableException();

            ValueData indexerDataB = indexerData.Value.LoadConst(Parser, indexerMethod.ParameterTypes[0]);
            valueData = valueData.LoadConst(Parser, indexerMethod.ParameterTypes[1]);

            Span<uint> parameters = v.Value.id != uint.MaxValue ? stackalloc uint[] {
                indexerDataB.Id, valueData.Id, v.Value.id
            } : stackalloc uint[] {
                indexerDataB.Id, valueData.Id,
            };

            il.Emit(OpCode.Call, uint.MaxValue, indexerMethod, parameters);
            return;
        }

        valueData = valueData.LoadConst(Parser, v.Value.type);
        if (!v.Value.isField)
            il.Emit(OpCode.Load, v.Value.id, valueData.Id);
        else
            il.Emit(OpCode.SetField, Parser.InstanceVariableId, v.Value.id, valueData.Id);
    }

    private (
        NeslType type, uint id, bool isField, NeslMethod? indexerMethod, ValueToken? indexer
    )? GetVariable(ValueToken assignment) {
        bool successful = true;

        ExpressionValueContentContainer container = null!;
        if (assignment.Value is ExpressionValueContentContainer c) {
            container = c;
        } else {
            successful = false;
            Parser.Throw(new CompilationError(
                assignment.Value.Pointer, CompilationErrorType.ExpectedAssignmentExpression, assignment.Value
            ));
        }

        if (assignment.LeftOperator != OperatorType.None) {
            successful = false;
            Parser.Throw(new CompilationError(
                assignment.Value.Pointer, CompilationErrorType.LeftOperatorNotAllowed, assignment.Value
            ));
        }
        if (assignment.RightOperator != OperatorType.None) {
            successful = false;
            Parser.Throw(new CompilationError(
                assignment.Value.Pointer, CompilationErrorType.OperatorNotAllowed, assignment.Value
            ));
        }

        if (!successful)
            return null;

        ValueData? value = null;
        if (container.Expressions.Count != 1) {
            value = ValueConstructor.Construct(new ValueToken(
                OperatorType.None, new ExpressionValueContentContainer(container.Expressions.SkipLast(1).ToArray()),
                null, null
            ), Parser);
        }

        ExpressionValueContent expression = container.Expressions[^1];
        if (expression.IsNew) {
            successful = false;
            Parser.Throw(new CompilationError(
                assignment.Value.Pointer, CompilationErrorType.AssignmentExpressionCannotBeNew, assignment.Value
            ));
        }
        if (expression.RoundBrackets is not null || expression.CurlyBrackets is not null) {
            successful = false;
            Parser.Throw(new CompilationError(
                assignment.Value.Pointer, CompilationErrorType.AssignmentExpressionCannotBeMethod, assignment.Value
            ));
        }
        if (expression.Identifier.HasValue && expression.Identifier.Value.GenericTokens.Any()) {
            successful = false;
            Parser.Throw(new CompilationError(
                expression.Identifier!.Value.GenericTokens[0].Pointer, CompilationErrorType.AssignmentGenericNotAllowed,
                expression.Identifier!.Value
            ));
        }

        if (!successful)
            return null;

        (NeslType type, uint id, bool isField, NeslMethod? indexerMethod)? fragment =
            GetVariableFragment(value, expression.Identifier!.Value);
        if (fragment is null)
            return null;

        if (fragment.Value.indexerMethod is not null && expression.Identifier is null) {
            Parser.Throw(new CompilationError(
                expression.Identifier!.Value.Pointer, CompilationErrorType.ExpectedIndexer, expression.Identifier!.Value
            ));
            return null;
        }

        return (
            fragment.Value.type, fragment.Value.id, fragment.Value.isField, fragment.Value.indexerMethod,
            expression.Indexer
        );
    }

    private (NeslType type, uint id, bool isField, NeslMethod? indexerMethod)? GetVariableFragment(
        ValueData? value, TypeIdentifierToken identifier
    ) {
        string[] split = identifier.Identifier.Split('.');
        string element = split[0];

        NeslType? type = null;
        uint id = 0;

        if (value is null) {
            VariableData? data = Parser.GetVariable(element);
            if (data is not null)
                (type, id) = (data.Value.Type, data.Value.Id);
        } else {
            (type, id) = (value.Value.Type, value.Value.Id);
        }

        int start = 1;
        if (type is null) {
            start = 0;
            type = Parser.CurrentMethod.Type;
        }

        bool isField = false;
        for (int i = start; i < split.Length; i++) {
            element = split[i];

            uint a = 0;
            bool isBreaked = false;
            foreach (NeslField field in type.Fields) {
                if (field.Name == element) {
                    type = field.FieldType;
                    id = a;
                    isField = true;
                    isBreaked = true;
                    break;
                }
                a++;
            }

            // TODO: Add properties.

            if (!isBreaked) {
                if (i == split.Length - 1) {
                    string name = NeslOperators.IndexerSet;
                    if (element != "this")
                        name += element;

                    NeslMethod? indexerMethod = type.GetMethod(name);
                    if (indexerMethod is not null)
                        return (type, id, false, indexerMethod);
                }

                Parser.Throw(new CompilationError(
                    identifier.Pointer, CompilationErrorType.VariableOrFieldOrPropertyNotFound, element
                ));
                return null;
            }
        }

        return (type!, id, isField, null);
    }

}
