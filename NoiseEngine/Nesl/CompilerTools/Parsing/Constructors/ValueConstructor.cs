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

            foreach (ExpressionValueContent expression in container.Expressions) {
                if (expression.IsNew)
                    return ConstructNew(parser, expression);
                return ConstructValue(parser, expression);
            }
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

    private static ValueData ConstructValue(Parser parser, ExpressionValueContent expression) {
        if (expression.Identifier is null)
            throw new UnreachableException();
        TypeIdentifierToken identifier = expression.Identifier.Value;

        int index = identifier.Identifier.IndexOf('.');
        string fragmentName = index != -1 ? identifier.Identifier[..index] : identifier.Identifier;

        VariableData? variable = parser.GetVariable(fragmentName);
        if (variable is null)
            return CallMethod(parser, expression);

        if (index == -1)
            return new ValueData(variable.Value.Type, variable.Value.Id);

        NeslType type = variable.Value.Type;
        NeslField? field;

        IlGenerator il = parser.CurrentMethod.IlGenerator;
        uint variableId = variable.Value.Id;

        int sum = index + 1;
        while (true) {
            index = identifier.Identifier.AsSpan(sum).IndexOf('.');
            string str = index != -1 ? identifier.Identifier[sum..(sum + index)] : identifier.Identifier[sum..];

            field = type.GetField(str);
            if (field is null)
                throw new NotImplementedException();

            il.Emit(OpCode.DefVariable, field.FieldType);
            uint newVariableId = il.GetNextVariableId();
            il.Emit(OpCode.LoadField, newVariableId, variableId, field.Id);
            variableId = newVariableId;

            if (index == -1)
                break;

            type = field.FieldType;
            sum += index + 1;
        }

        if (field is null)
            throw new NotImplementedException();
        return new ValueData(field.FieldType, variableId);
    }

    private static ValueData CallMethod(Parser parser, ExpressionValueContent expression) {
        TypeIdentifierToken identifier = expression.Identifier!.Value;
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

        List<ValueData>? parameters = GetParameters(parser, expression.RoundBrackets!.Value.Buffer);
        if (parameters is null)
            return ValueData.Invalid;

        NeslMethod? method = null;
        foreach (NeslMethod m in methods) {
            if (m.ParameterTypes.SequenceEqual(parameters.Select(x => x.Type))) {
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
        il.Emit(OpCode.Call, variableId, method, parameters.Select(x => x.Id).ToArray());
        return new ValueData(method.ReturnType, variableId);
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
