using NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;
using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;
using NoiseEngine.Nesl.Emit;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace NoiseEngine.Nesl.CompilerTools.Parsing;

internal class Parser {

    private readonly List<CompilationError> errors = new List<CompilationError>();

    private List<string>? usings;
    private List<(NeslTypeBuilder, TokenBuffer)>? definedTypes;
    private List<(TypeIdentifierToken typeIdentifier, string name)>? definedParameters;
    private List<(
        TypeIdentifierToken typeIdentifier, NameToken name, TokenBuffer parameters, TokenBuffer codeBlock
    )>? definedMethods;
    private List<Parser>? types;

    private NeslTypeBuilder? currentType;
    private NeslMethodBuilder? currentMethod;

    public Parser? Parent { get; }
    public NeslAssemblyBuilder Assembly { get; }
    public ParserStep Step { get; }
    public TokenBuffer Buffer { get; }
    public IReadOnlyList<CompilationError> Errors => errors;
    public CompilationErrorMode ErrorMode { get; } = new CompilationErrorMode();

    public NeslMethodBuilder CurrentMethod {
        get {
            if (currentMethod is null)
                currentMethod = CurrentType.DefineMethod(Guid.NewGuid().ToString());
            return currentMethod;
        }
        init {
            currentMethod = value;
        }
    }

    private NeslTypeBuilder CurrentType {
        get {
            if (currentType is null)
                currentType = Assembly.DefineType(Guid.NewGuid().ToString());
            return currentType;
        }
        init {
            currentType = value;
        }
    }

    private IEnumerable<string> Usings {
        get {
            if (usings is not null) {
                foreach (string u in usings)
                    yield return u;
            }

            if (Parent is not null) {
                foreach (string u in Parent.Usings)
                    yield return u;
            }
        }
    }

    public Parser(Parser? parent, NeslAssemblyBuilder assembly, ParserStep step, TokenBuffer buffer) {
        Parent = parent;
        Assembly = assembly;
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
        List<(ParserExpression expression, int prioritySum)> compabilityExpressions =
            new List<(ParserExpression, int)>();
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
                int prioritySum = 0;
                foreach ((MethodInfo method, _) in expression.ExpectedTokens) {
                    i = Buffer.Index;
                    if (!tokens.TryGetValue((i, method), out (IParserToken token, int length) o)) {
                        bool result = (bool)(
                            method.Invoke(null, parseParameters) ?? throw new NullReferenceException()
                        );
                        if (!result) {
                            isBreaked = true;
                            parseParameters[2] = null;
                            break;
                        }

                        o = (
                            (IParserToken)(parseParameters[2] ?? throw new NullReferenceException()), Buffer.Index - i
                        );
                        tokens.Add((i, method), o);
                        parseParameters[2] = null;
                    } else {
                        Buffer.Index += o.length;
                    }

                    if (!o.token.IsIgnored) {
                        compabilityCount++;
                        prioritySum += o.token.Priority;
                    }
                }

                if (compabilityCount != 0 && compabilityCount > mostCompabilityCount) {
                    mostCompabilityExpression = expression;
                    mostCompabilityCount = compabilityCount;
                    mostCompabilityIndex = i;
                }

                if (!isBreaked)
                    compabilityExpressions.Add((expression, prioritySum));
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
                    parseParameters[2] = null;

                    errors.Add((CompilationError)(parseParameters[3] ?? throw new NullReferenceException()));
                    Buffer.Index = mostCompabilityIndex;
                }
                continue;
            }

            ParserExpression finalExpression = compabilityExpressions
                .OrderByDescending(x => x.prioritySum).ThenByDescending(x => x.expression.ExpectedTokens.Count)
                .First().expression;

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

    public void AnalyzeTypes() {
        if (definedTypes is null)
            return;

        types = new List<Parser>();
        foreach ((NeslTypeBuilder typeBuilder, TokenBuffer buffer) in definedTypes) {
            Parser parser = new Parser(this, Assembly, ParserStep.Type, buffer) {
                CurrentType = typeBuilder
            };
            parser.Parse();
            parser.AnalyzeTypes();

            foreach (CompilationError error in parser.Errors)
                errors.Add(error);

            types.Add(parser);
        }

        definedTypes = null;
    }

    public void AnalyzeFields() {
        if (definedParameters is not null) {
            foreach ((TypeIdentifierToken typeIdentifier, string name) in definedParameters) {
                if (!TryGetType(typeIdentifier, out NeslType? fieldType))
                    continue;

                CurrentType.DefineField(name, fieldType);
            }

            // Add default constructor to value type.
            if (CurrentType.IsValueType) {
                IlGenerator il = CurrentType.DefineMethod(NeslOperators.Constructor, CurrentType).IlGenerator;
                il.Emit(Emit.OpCode.DefVariable, CurrentType);
                il.Emit(Emit.OpCode.ReturnValue, il.GetNextVariableId());
            }

            definedParameters = null;
        }

        if (types is not null) {
            foreach (Parser typeParser in types) {
                typeParser.AnalyzeFields();

                foreach (CompilationError error in typeParser.Errors)
                    errors.Add(error);
            }
        }
    }

    public void AnalyzeMethods() {
        if (definedMethods is not null) {
            foreach (
                (TypeIdentifierToken typeIdentifier, NameToken name, TokenBuffer parameters, TokenBuffer codeBlock)
                in definedMethods
            ) {
                // Parameters.
                TokenBuffer newParameters;
                if (parameters.Tokens.Count == 0) {
                    newParameters = parameters;
                } else {
                    Token last = parameters.Tokens[^1];
                    newParameters = new TokenBuffer(parameters.Tokens.Append(new Token(
                        last.Path, last.Line, last.Column + 1, TokenType.Comma, 1, null
                    )).ToArray());
                }

                ParameterParser parameterParser = new ParameterParser(
                    this, Assembly, ParserStep.Parameters, newParameters
                );
                parameterParser.Parse();

                foreach (CompilationError error in parameterParser.Errors)
                    errors.Add(error);

                // Return type.
                NeslType? returnType = null;
                if (typeIdentifier.Identifier != "void")
                    TryGetType(typeIdentifier, out returnType);

                // Construct.
                if (!CurrentType.TryDefineMethod(
                    name.Name, out NeslMethodBuilder? method, returnType,
                    parameterParser.DefinedParameters.Select(x => x.type).ToArray()
                )) {
                    Throw(new CompilationError(
                        name.Pointer, CompilationErrorType.MethodAlreadyExists, name.Name
                    ));
                    return;
                }

                Parser methodParser = new Parser(this, Assembly, ParserStep.Method, codeBlock) {
                    CurrentType = CurrentType,
                    CurrentMethod = method!
                };
                methodParser.Parse();

                foreach (CompilationError error in methodParser.Errors)
                    errors.Add(error);
            }

            definedMethods = null;
        }
    }

    public void Throw(CompilationError error) {
        errors.Add(error);
    }

    public bool TryDefineUsing(string u) {
        usings ??= new List<string>();
        foreach (string u2 in usings) {
            if (u == u2)
                return false;
        }
        usings.Add(u);
        return true;
    }

    public void DefineType(NeslTypeBuilder typeBuilder, TokenBuffer buffer) {
        definedTypes ??= new List<(NeslTypeBuilder, TokenBuffer)>();
        definedTypes.Add((typeBuilder, buffer));
    }

    public bool TryDefineField(TypeIdentifierToken typeIdentifier, string name) {
        definedParameters ??= new List<(TypeIdentifierToken typeIdentifier, string name)>();
        foreach ((_, string n) in definedParameters) {
            if (name == n)
                return false;
        }
        definedParameters.Add((typeIdentifier, name));
        return true;
    }

    public void DefineMethod(
        TypeIdentifierToken typeIdentifier, NameToken name, TokenBuffer parameters, TokenBuffer codeBlock
    ) {
        definedMethods ??= new List<(TypeIdentifierToken, NameToken, TokenBuffer, TokenBuffer)>();
        definedMethods.Add((typeIdentifier, name, parameters, codeBlock));
    }

    public bool TryGetType(TypeIdentifierToken typeIdentifier, [NotNullWhen(true)] out NeslType? type) {
        type = typeIdentifier.GetTypeFromAssembly(Assembly, Usings);
        if (type is not null)
            return true;

        Throw(new CompilationError(
            typeIdentifier.Pointer, CompilationErrorType.TypeNotFound, typeIdentifier.Identifier
        ));
        return false;
    }

}
