using NoiseEngine.Nesl.CompilerTools.Generics;
using NoiseEngine.Nesl.Emit.Attributes.Internal;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace NoiseEngine.Nesl.CompilerTools.Parsing;

internal sealed class ParserStorage {

    private readonly ConcurrentDictionary<NeslType, ConcurrentBag<CodePointer[]>> genericMakedTypes =
        new ConcurrentDictionary<NeslType, ConcurrentBag<CodePointer[]>>();
    private readonly ConcurrentDictionary<NeslMethod, Parser> methodParsers =
        new ConcurrentDictionary<NeslMethod, Parser>();

    public void AddGenericMakedType(NeslType type, CodePointer[] pointers) {
        Debug.Assert(type.GenericMakedFrom is not null);
        genericMakedTypes.GetOrAdd(type, _ => new ConcurrentBag<CodePointer[]>()).Add(pointers);
    }

    public void AddMethodParser(NeslMethod method, Parser parser) {
        methodParsers.TryAdd(method, parser);
    }

    public Parser? GetMethodParser(NeslMethod method) {
        if (methodParsers.TryGetValue(method, out Parser? parser))
            return parser;
        if (
            method.IsAbstract ||
            (method.Name == NeslOperators.Constructor && method.ParameterTypes.Count == 0) ||
            method.Attributes.HasAnyAttribute(IntrinsicAttribute.Create().FullName) ||
            method.Attributes.HasAnyAttribute(CallOpCodeAttribute.Create(0).FullName)
        ) {
            return null;
        }

        throw new UnreachableException();
    }

    public void CheckGenericConstraintSatisfying(Parser anyParser) {
        Parallel.ForEach(genericMakedTypes, x => {
            int i = 0;
            foreach (NeslType genericType in x.Key.GenericMakedTypeParameters) {
                NeslGenericTypeParameter genericTypeParameter =
                    x.Key.GenericMakedFrom!.GenericTypeParameters.ElementAt(i);

                bool isSatisfied = true;
                foreach (NeslType constraint in genericTypeParameter.Interfaces) {
                    NeslType c = constraint;
                    if (c is NotFullyConstructedGenericNeslType notFully) {
                        c = c.MakeGeneric(notFully.GenericMakedTypeParameters.Select(y => {
                            if (y is NeslGenericTypeParameter p) {
                                int i = x.Key.GenericMakedFrom.GenericTypeParameters.Select((z, i) => (z, i))
                                    .First(z => z.z == p).i;
                                return x.Key.GenericMakedTypeParameters.ElementAt(i);
                            }
                            return y;
                        }).ToArray());
                    }

                    if (!genericType.Interfaces.Contains(c)) {
                        isSatisfied = false;
                        break;
                    }
                }

                if (!isSatisfied) {
                    foreach (CodePointer[] pointers in x.Value) {
                        anyParser.Throw(new CompilationError(
                            pointers[i], CompilationErrorType.TypeNotSatisfiedGenericConstraint, genericType.Name
                        ));
                    }
                }
                i++;
            }
        });

        genericMakedTypes.Clear();
    }

}
