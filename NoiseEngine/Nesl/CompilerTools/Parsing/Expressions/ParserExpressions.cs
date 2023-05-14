using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;

internal static class ParserExpressions {

    private static readonly Type[] containers;
    private static readonly Dictionary<ParserStep, ParserExpression[]> expressions;

    static ParserExpressions() {
        containers = typeof(ParserExpressionContainer).Assembly.GetTypes().Where(x =>
            x.Namespace == typeof(ParserExpressionContainer).Namespace &&
            x != typeof(ParserExpressionContainer) && typeof(ParserExpressionContainer).IsAssignableFrom(x)
        ).ToArray();

        Dictionary<ParserStep, List<ParserExpression>> expressions =
            new Dictionary<ParserStep, List<ParserExpression>>();
        List<(MethodInfo, bool)> expectedTokens = new List<(MethodInfo, bool)>();

        foreach (MethodInfo methodInfo in containers.SelectMany(x => x.GetMethods())) {
            ParserExpressionAttribute? expressionAttribute =
                methodInfo.GetCustomAttribute<ParserExpressionAttribute>();
            if (expressionAttribute is null)
                continue;

            expectedTokens.Clear();
            foreach (Attribute attribute in methodInfo.GetCustomAttributes()) {
                ParserTokenType tokenType;
                bool isParameter;

                if (attribute is ParserExpressionParameterAttribute parameter) {
                    tokenType = parameter.TokenType;
                    isParameter = true;
                } else if (attribute is ParserExpressionTokenTypeAttribute token) {
                    tokenType = token.TokenType;
                    isParameter = false;
                } else {
                    continue;
                }

                MethodInfo? m = tokenType.GetCustomAttribute<ParserTokenAttribute>().Type.GetMethod("Parse");
                if (m is null)
                    throw new InvalidOperationException();
                expectedTokens.Add((m, isParameter));
            }

            ParserExpression expression = new ParserExpression(methodInfo, expectedTokens.ToArray());

            foreach (ParserStep step in Enum.GetValues<ParserStep>()) {
                if (!expressionAttribute.Step.HasFlag(step))
                    continue;

                if (!expressions.TryGetValue(step, out List<ParserExpression>? list)) {
                    list = new List<ParserExpression>();
                    expressions.Add(step, list);
                }

                list.Add(expression);
            }
        }

        ParserExpressions.expressions = new Dictionary<ParserStep, ParserExpression[]>();
        foreach ((ParserStep step, List<ParserExpression> expression) in expressions)
            ParserExpressions.expressions.Add(step, expression.ToArray());
    }

    public static IReadOnlyList<ParserExpression> GetExpressions(ParserStep step) {
        if (expressions.TryGetValue(step, out ParserExpression[]? e))
            return e;
        return Array.Empty<ParserExpression>();
    }

}
