using System;
using System.Collections.Generic;
using System.Reflection;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;

internal class ParserExpression {

    public MethodInfo Method { get; }
    public IReadOnlyList<(MethodInfo creator, bool isParameter)> ExpectedTokens { get; }

    public ParserExpression(MethodInfo method, IReadOnlyList<(MethodInfo type, bool isParameter)> expectedTokens) {
        Method = method;
        ExpectedTokens = expectedTokens;
    }


}
