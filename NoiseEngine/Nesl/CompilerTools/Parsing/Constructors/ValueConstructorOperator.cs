using NoiseEngine.Nesl.CompilerTools.Generics;
using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;
using NoiseEngine.Nesl.Emit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Constructors;

internal static class ValueConstructorOperator {

    private static readonly Dictionary<OperatorType, ValueConstructorOperatorData> data =
        new Dictionary<OperatorType, ValueConstructorOperatorData>() {
        { OperatorType.Addition, new ValueConstructorOperatorData("IAdd`3", "Add") },
        { OperatorType.Subtraction, new ValueConstructorOperatorData("ISubtract`3", "Subtract") },
        { OperatorType.Multiplication, new ValueConstructorOperatorData("IMultiply`3", "Multiply") },
        { OperatorType.Division, new ValueConstructorOperatorData("IDivide`3", "Divide") },
        { OperatorType.Remainder, new ValueConstructorOperatorData("IRemainder`3", "Remainder") },
        { OperatorType.Exponentation, new ValueConstructorOperatorData("IPower`3", "Power") }
    };

    public static ValueData Construct(ValueNode node, Parser parser) {
        ValueData lhs = ValueConstructor.Construct(node.Left, parser);
        ValueData rhs = ValueConstructor.Construct(node.Right, parser);

        if (lhs.IsInvalid || rhs.IsInvalid)
            return ValueData.Invalid;

        if (!ValueConstructorOperator.data.TryGetValue(node.Operator.Type, out ValueConstructorOperatorData? data))
            throw new NotImplementedException();

        // Find interface.
        IEnumerable<(bool, NeslType)> interfaces = Enumerable.Empty<(bool, NeslType)>();
        interfaces = interfaces.Concat((lhs.Type ?? lhs.GetBestMatchConstType()).Interfaces.Select(x => (true, x)));
        interfaces = interfaces.Concat((rhs.Type ?? lhs.GetBestMatchConstType()).Interfaces.Select(x => (false, x)));

        if (
            lhs.Type is NeslGenericTypeParameter lhsGeneric &&
            parser.CurrentMethod.TypeGenericConstraints.TryGetValue(lhsGeneric, out IReadOnlyList<NeslType>? ti)
        ) {
            interfaces = interfaces.Concat(ti.Select(x => (true, x)));
        }
        if (
            rhs.Type is NeslGenericTypeParameter rhsGeneric &&
            parser.CurrentMethod.TypeGenericConstraints.TryGetValue(rhsGeneric, out ti)
        ) {
            interfaces = interfaces.Concat(ti.Select(x => (false, x)));
        }

        interfaces = interfaces.Where(x =>
            x.Item2.FullNameWithAssembly == data.InterfaceFullNameWithAssembly &&
            (lhs.Type is null ? lhs.CheckLoadConst(x.Item2.GenericMakedTypeParameters.First()) :
                (lhs.Type == x.Item2.GenericMakedTypeParameters.First())) &&
            (rhs.Type is null ? rhs.CheckLoadConst(x.Item2.GenericMakedTypeParameters.Skip(1).First()) :
                (rhs.Type == x.Item2.GenericMakedTypeParameters.Skip(1).First()))
        );

        (bool, NeslType?) i = interfaces.FirstOrDefault();
        if (i.Item2 is null) {
            parser.Throw(new CompilationError(
                node.Operator.Pointer, CompilationErrorType.OperatorNotFound, node.Operator.Type
            ));
            return ValueData.Invalid;
        }

        lhs = lhs.LoadConst(parser, i.Item2.GenericMakedTypeParameters.First());
        rhs = rhs.LoadConst(parser, i.Item2.GenericMakedTypeParameters.Skip(1).First());

        NeslType type = i.Item1 ? lhs.Type! : rhs.Type!;

        IEnumerable<NeslMethod> methods = type.Methods;
        if (
            type is NeslGenericTypeParameterBuilder genericType &&
            parser.CurrentMethod.TypeGenericConstraints.TryGetValue(
                genericType, out IReadOnlyList<NeslType>? typeGenericConstraints
            )
        ) {
            foreach (NeslType genericConstraint in typeGenericConstraints) {
                if (genericConstraint is IGenericMakedForInitialize forInitialize)
                    parser.Storage.InitializeGenericMakedType(parser, forInitialize);
                methods = methods.Concat(genericType.GetOrAddNestedConstraint(genericConstraint));
            }
        }

        NeslMethod? method = methods.FirstOrDefault(x =>
            x.Name == data.MethodName && x.ParameterTypes.Count == 2 &&
            x.ParameterTypes[0] == lhs.Type && x.ParameterTypes[1] == rhs.Type
        );
        if (method is not null) {
            return ValueConstructor.CallMethod(parser, method, null, new ValueData[] {
                lhs, rhs
            });
        }

        Console.WriteLine("dasdsd " + parser.CurrentMethod.FullName);
        Console.WriteLine(type is NeslGenericTypeParameterBuilder);
        if (type is NeslGenericTypeParameterBuilder genericType2) {
            foreach (var a in parser.CurrentMethod.TypeGenericConstraints) {
                Console.WriteLine("fd " + a.Key.FullName);
                foreach (var b in a.Value) {
                    Console.WriteLine(b.FullName + " " + b.GenericMakedTypeParameters.First().FullName + " methods: " + b.Methods.Count());
                }
            }
        }

        // Unhappy path.
        StringBuilder builder = new StringBuilder("Unable to find NESL method of operator `");
        builder.Append(i.Item2.FullName);
        builder.Append("` with generic maked type parameters (");

        foreach (NeslType genericMakedTypeParameter in i.Item2.GenericMakedTypeParameters) {
            builder.Append(genericMakedTypeParameter.FullName);
            builder.Append(", ");
        }
        builder.Remove(builder.Length - 2, 2);

        builder.Append(") for type named `");
        builder.Append(type.FullName);
        builder.Append("`.");

        throw new UnreachableException(builder.ToString());
    }

}
