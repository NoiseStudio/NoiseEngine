using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;
using NoiseEngine.Nesl.Emit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Constructors;

internal static class ValueConstructor {

    public static ValueData Construct(ValueToken value, Parser parser) {
        IValueNodeElement element = GetNodeElement(value);
        if (element is ValueToken token) {
            if (token.Value is ValueToken innerToken)
                return Construct(innerToken, parser);
            if (token.Value is ConstValueToken constValue)
                return new ValueData(null!, uint.MaxValue, constValue);
            if (token.Value is not ExpressionValueContentContainer container)
                throw new UnreachableException();

            ValueData data = ValueData.Invalid;
            foreach (ExpressionValueContent expression in container.Expressions) {
                if (expression.IsNew) {
                    if (!data.IsInvalid)
                        throw new UnreachableException();
                    data = ConstructNew(parser, expression);
                } else {
                    ConstructValue(ref data, parser, expression);
                }

                if (expression.Indexer is not null)
                    CallIndexer(ref data, parser, expression.Indexer, NeslOperators.IndexerGet);
            }
            return data;
        }

        return ValueData.Invalid;
    }

    private static ValueData ConstructNew(Parser parser, ExpressionValueContent expression) {
        if (expression.Identifier is null)
            throw new UnreachableException();
        TypeIdentifierToken identifier = expression.Identifier.Value;

        if (!parser.TryGetType(identifier, out NeslType? type))
            return ValueData.Invalid;

        List<ValueData>? parameters = GetParameters(parser, expression.RoundBrackets!.Value.Buffer);
        if (parameters is null)
            return ValueData.Invalid;

        NeslMethod? constructor = null;
        foreach (NeslMethod m in type.Methods.Where(x => x.Name == NeslOperators.Constructor)) {
            if (m.ParameterTypes.SequenceEqual(parameters.Select(x => x.Type))) {
                constructor = m;
                break;
            }
        }

        if (constructor is null) {
            parser.Throw(new CompilationError(
                identifier.Pointer, CompilationErrorType.ConstructorNotFound, identifier.Identifier
            ));
            return ValueData.Invalid;
        }

        IlGenerator il = parser.CurrentMethod.IlGenerator;
        il.Emit(OpCode.DefVariable, type);
        uint variableId = il.GetNextVariableId();
        il.Emit(OpCode.Call, variableId, constructor, parameters.Select(x => x.Id).ToArray());

        if (expression.CurlyBrackets is not null)
            NewInitializer.Initialize(parser, type, variableId, expression.CurlyBrackets.Value.Buffer);

        return new ValueData(type, variableId);
    }

    private static void ConstructValue(ref ValueData data, Parser parser, ExpressionValueContent expression) {
        if (expression.Identifier is null)
            throw new UnreachableException();
        TypeIdentifierToken identifier = expression.Identifier.Value;

        int index;
        int sum;
        if (data.IsInvalid) {
            index = identifier.Identifier.IndexOf('.');
            string fragmentName = index != -1 ? identifier.Identifier[..index] : identifier.Identifier;

            VariableData? variable = parser.GetVariable(fragmentName);
            if (variable is null) {
                uint i = 0;
                foreach (NeslField f in parser.CurrentMethod.Type.Fields) {
                    if (f.Name == fragmentName) {
                        variable = new VariableData(f.FieldType, f.Name, i);
                        break;
                    }
                    i++;
                }

                // TODO: Add properties.

                if (variable is null) {
                    if (expression.RoundBrackets is null) {
                        parser.Throw(new CompilationError(
                            identifier.Pointer, CompilationErrorType.VariableOrFieldOrPropertyNotFound,
                            identifier.Identifier
                        ));
                        data = ValueData.Invalid;
                        return;
                    }

                    data = CallLocalOrStaticMethod(
                        parser, expression.Identifier.Value, expression.RoundBrackets.Value.Buffer
                    );
                    return;
                }
            }

            if (index == -1) {
                data = new ValueData(variable.Value.Type, variable.Value.Id);
                return;
            }

            data = new ValueData(variable.Value.Type, variable.Value.Id);
            sum = index + 1;
        } else {
            index = 0;
            sum = 0;
        }

        NeslType type = data.Type;
        uint variableId = data.Id;
        IlGenerator il = parser.CurrentMethod.IlGenerator;

        while (true) {
            if (index == -1)
                break;

            index = identifier.Identifier.AsSpan(sum).IndexOf('.');
            string str = index != -1 ? identifier.Identifier[sum..(sum + index)] : identifier.Identifier[sum..];

            if (index != -1 || expression.RoundBrackets is null) {
                // Get field.
                if (type.Kind != NeslTypeKind.GenericParameter) {
                    NeslField? field = type.GetField(str);
                    if (field is not null) {
                        il.Emit(OpCode.DefVariable, field.FieldType);
                        uint newVariableId = il.GetNextVariableId();
                        il.Emit(OpCode.LoadField, newVariableId, variableId, field.Id);
                        variableId = newVariableId;

                        type = field.FieldType;
                        sum += index + 1;
                        continue;
                    }
                }

                // TODO: Add properties.

                parser.Throw(new CompilationError(
                    identifier.Pointer, CompilationErrorType.VariableOrFieldOrPropertyNotFound, str
                ));
                data = ValueData.Invalid;
                return;
            }

            // Call method.
            IEnumerable<NeslMethod> methods = type.Methods.Where(x => x.Name == str);
            if (!methods.Any()) {
                parser.Throw(new CompilationError(
                    identifier.Pointer, CompilationErrorType.MethodNotFound, identifier.Identifier
                ));
                data = ValueData.Invalid;
                return;
            }

            data = CallMethod(
                parser, expression.Identifier.Value with { Identifier = str }, expression.RoundBrackets.Value.Buffer,
                variableId, methods
            );
            return;
        }

        data = new ValueData(type, variableId);
    }

    private static ValueData CallLocalOrStaticMethod(
        Parser parser, TypeIdentifierToken identifier, TokenBuffer parameters
    ) {
        if (!parser.TryGetMethods(identifier, out bool findedType, out IEnumerable<NeslMethod>? methods)) {
            if (findedType) {
                parser.Throw(new CompilationError(
                    identifier.Pointer, CompilationErrorType.MethodNotFound, identifier.Identifier
                ));
            } else {
                parser.Throw(new CompilationError(
                    identifier.Pointer, CompilationErrorType.TypeNotFound, identifier.Identifier
                ));
            }
            return ValueData.Invalid;
        }

        return CallMethod(parser, identifier, parameters, null, methods);
    }

    private static ValueData CallMethod(
        Parser parser, TypeIdentifierToken identifier, TokenBuffer parameters, uint? instanceId,
        IEnumerable<NeslMethod> methods
    ) {
        List<ValueData>? parametersList = GetParameters(parser, parameters);
        if (parametersList is null)
            return ValueData.Invalid;

        NeslMethod? method = null;
        foreach (NeslMethod m in methods) {
            if (m.ParameterTypes.SequenceEqual(parametersList.Select(x => x.Type))) {
                method = m;
                break;
            }
        }

        if (method is null) {
            parser.Throw(new CompilationError(
                identifier.Pointer, CompilationErrorType.MethodWithGivenArgumentsNotFound, identifier.Identifier
            ));
            return ValueData.Invalid;
        }

        if (method.ReturnType is null)
            throw new NotImplementedException();

        IlGenerator il = parser.CurrentMethod.IlGenerator;
        il.Emit(OpCode.DefVariable, method.ReturnType);
        uint variableId = il.GetNextVariableId();

        int start = instanceId is null ? 0 : 1;
        Span<uint> parametersIds = stackalloc uint[start + parametersList.Count];
        for (int i = start; i < parametersIds.Length; i++)
            parametersIds[i] = parametersList[i - start].Id;

        if (instanceId is not null)
            parametersIds[0] = instanceId.Value;

        il.Emit(OpCode.Call, variableId, method, parametersIds);
        return new ValueData(method.ReturnType, variableId);
    }

    private static void CallIndexer(ref ValueData data, Parser parser, ValueToken indexer, string name) {
        ValueData indexerData = Construct(indexer, parser);

        NeslMethod? method = data.Type.GetMethod(name);
        if (method is null) {
            parser.Throw(new CompilationError(indexer.Pointer, CompilationErrorType.IndexerNotFound, name));
            return;
        }

        indexerData = indexerData.LoadConst(parser, method.ParameterTypes[0]);
        if (method.ReturnType is null)
            throw new UnreachableException();

        IlGenerator il = parser.CurrentMethod.IlGenerator;
        il.Emit(OpCode.DefVariable, method.ReturnType);
        uint variableId = il.GetNextVariableId();
        il.Emit(OpCode.Call, variableId, method, stackalloc uint[] {
            data.Id, indexerData.Id
        });
        data = new ValueData(method.ReturnType, variableId);
    }

    private static List<ValueData>? GetParameters(Parser parser, TokenBuffer buffer) {
        List<ValueData> list = new List<ValueData>();
        if (!buffer.HasNextTokens)
            return list;

        while (true) {
            if (!ValueToken.Parse(buffer, parser.ErrorMode, out ValueToken? value, out CompilationError error)) {
                parser.Throw(error);
                return null;
            }

            list.Add(Construct(value, parser));

            if (!buffer.HasNextTokens)
                break;

            if (!buffer.TryReadNext(TokenType.Comma, out Token token)) {
                parser.Throw(new CompilationError(token, CompilationErrorType.ExpectedComma));
                return null;
            }
        }
        return list;
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
