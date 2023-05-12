using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;
using NoiseEngine.Nesl.Emit;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Constructors;

internal static class ValueConstructor {

    public static uint Construct(ValueToken value, Parser parser) {
        IValueNodeElement element = GetNodeElement(value);
        if (element is ValueToken token) {
            if (token.Value is ValueToken innerToken)
                return Construct(innerToken, parser);
            if (token.Value is not ExpressionValueContentContainer container)
                throw new UnreachableException();

            foreach (ExpressionValueContent expression in container.Expressions) {
                TypeIdentifierToken identifier;
                if (expression.IsNew) {
                    if (expression.Identifier is null)
                        throw new UnreachableException();
                    identifier = expression.Identifier.Value;
                } else {
                    throw new NotImplementedException();
                }

                if (!parser.TryGetType(identifier, out NeslType? type)) {
                    parser.Throw(new CompilationError(
                        identifier.Pointer, CompilationErrorType.TypeNotFound, identifier.Identifier
                    ));
                    return uint.MaxValue;
                }

                NeslMethod? constructor = type.GetMethod(NeslOperators.Constructor);
                if (constructor is null) {
                    parser.Throw(new CompilationError(
                        identifier.Pointer, CompilationErrorType.ConstructorNotFound, identifier.Identifier
                    ));
                    return uint.MaxValue;
                }

                IlGenerator il = parser.CurrentMethod.IlGenerator;
                il.Emit(OpCode.DefVariable, type);
                uint variableId = il.GetNextVariableId();
                il.Emit(OpCode.Call, variableId, constructor, Array.Empty<uint>());
                return variableId;
            }
        }

        return uint.MaxValue;
    }

    private static IValueNodeElement GetNodeElement(ValueToken value) {
        if (value.NextValue is null)
            return value;

        List<(IValueNodeElement, OperatorType)> values = new List<(IValueNodeElement, OperatorType)>() {
            (value, OperatorType.None)
        };
        while (value.NextValue is not null) {
            value = value.NextValue;
            values[^1] = (values[^1].Item1, value.Operator);
            values.Add((value, OperatorType.None));
        }

        while (values.Count > 1) {
            int priority = -1;
            int index = -1;
            for (int i = 0; i < values.Count; i++) {
                (_, OperatorType o) = values[i];
                int p = (int)o / 100;
                if (p > priority) {
                    priority = p;
                    index = i;
                }
            }

            (IValueNodeElement v1, OperatorType op1) = values[index];
            (IValueNodeElement v2, OperatorType op2) = values[index + 1];

            ValueNode node = new ValueNode(v1, op1, v2);
            values[index] = (node, op2);
            values.RemoveAt(index + 1);
        }

        Debug.Assert(values[0].Item2 == OperatorType.None);
        return values[0].Item1;
    }

}
