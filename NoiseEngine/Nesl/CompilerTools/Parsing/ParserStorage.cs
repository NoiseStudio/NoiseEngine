using System.Collections.Concurrent;
using System.Diagnostics;

namespace NoiseEngine.Nesl.CompilerTools.Parsing;

internal sealed class ParserStorage {

    private readonly ConcurrentDictionary<NeslMethod, Parser> methodParsers =
        new ConcurrentDictionary<NeslMethod, Parser>();

    public void AddMethodParser(NeslMethod method, Parser parser) {
        methodParsers.TryAdd(method, parser);
    }

    public Parser? GetMethodParser(NeslMethod method) {
        if (methodParsers.TryGetValue(method, out Parser? parser))
            return parser;
        if (method.Name == NeslOperators.Constructor && method.ParameterTypes.Count == 0)
            return null;
        throw new UnreachableException();
    }

}
