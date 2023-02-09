using NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;
using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NoiseEngine.Nesl.CompilerTools.Parsing;

internal class Parser {

    private readonly List<CompilationError> errors = new List<CompilationError>();

    public ParserStep Step { get; }
    public TokenBuffer Buffer { get; }
    public IReadOnlyList<CompilationError> Errors => errors;

    public Parser(ParserStep step, TokenBuffer buffer) {
        Step = step;
        Buffer = buffer;
    }

    public void Parse() {
        object?[] parseParameters = new object?[] { Buffer, new CompilationErrorMode(), null, null };
        IReadOnlyList<ParserExpression> expressions = ParserExpressions.GetExpressions(Step);
        Dictionary<Type, ParserExpressionContainer> expressionContainers =
            new Dictionary<Type, ParserExpressionContainer>();

        Dictionary<(int index, MethodInfo method), (IParserToken token, int length)> tokens =
            new Dictionary<(int index, MethodInfo method), (IParserToken token, int length)>();
        List<ParserExpression> compabilityExpressions = new List<ParserExpression>();
        List<object?> parserExpressionParameters = new List<object?>();

        while (Buffer.HasNextTokens) {
            int index = Buffer.Index;

            tokens.Clear();
            compabilityExpressions.Clear();

            int mostCompabilityCount = 0;
            ParserExpression? mostCompabilityExpression = null;
            int mostCompabilityIndex = 0;

            foreach (ParserExpression expression in expressions) {
                Buffer.Index = index;
                int compabilityCount = 0;
                bool isBreaked = false;

                int i = 0;
                foreach ((MethodInfo method, _) in expression.ExpectedTokens) {
                    i = Buffer.Index;
                    if (!tokens.ContainsKey((i, method))) {
                        bool result = (bool)(
                            method.Invoke(null, parseParameters) ?? throw new NullReferenceException()
                        );
                        if (!result) {
                            isBreaked = true;
                            break;
                        }

                        tokens.Add((i, method), (
                            (IParserToken)(parseParameters[2] ?? throw new NullReferenceException()), Buffer.Index - i
                        ));
                    }

                    compabilityCount++;
                }

                if (compabilityCount > mostCompabilityCount) {
                    mostCompabilityExpression = expression;
                    mostCompabilityCount = compabilityCount;
                    mostCompabilityIndex = i;
                }

                if (!isBreaked)
                    compabilityExpressions.Add(expression);
            }

            if (compabilityExpressions.Count == 0) {
                if (mostCompabilityExpression is null) {
                    Buffer.Index = index;
                    _ = Buffer.TryReadNext(TokenType.None, out Token token);
                    errors.Add(new CompilationError(token, CompilationErrorType.UnexpectedExpression));
                } else {
                    Buffer.Index = mostCompabilityIndex;
                    (MethodInfo method, _) = mostCompabilityExpression.ExpectedTokens[mostCompabilityCount];

                    if ((bool)(
                        method.Invoke(null, parseParameters) ?? throw new NullReferenceException()
                    )) {
                        throw new InvalidOperationException();
                    }

                    errors.Add((CompilationError)(parseParameters[3] ?? throw new NullReferenceException()));
                    Buffer.Index = mostCompabilityIndex;
                }
                continue;
            }

            ParserExpression finalExpression =
                compabilityExpressions.OrderByDescending(x => x.ExpectedTokens.Count).First();

            int j = index;
            foreach ((MethodInfo creator, bool isParameter) in finalExpression.ExpectedTokens) {
                (IParserToken token, int length) = tokens[(j, creator)];
                j += length;

                if (isParameter)
                    parserExpressionParameters.Add(token);
            }
            Buffer.Index = j;

            Type expressionMethodType = finalExpression.Method.ReflectedType ?? throw new NullReferenceException();
            if (!expressionContainers.TryGetValue(expressionMethodType, out ParserExpressionContainer? container)) {
                container = (ParserExpressionContainer)(
                    Activator.CreateInstance(expressionMethodType, this) ?? throw new NullReferenceException()
                );
                expressionContainers.Add(expressionMethodType, container);
            }

            finalExpression.Method.Invoke(container, parserExpressionParameters.ToArray());
            parserExpressionParameters.Clear();
        }
    }

}
