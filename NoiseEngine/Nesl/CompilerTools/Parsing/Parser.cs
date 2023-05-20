using NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;
using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;
using NoiseEngine.Nesl.Emit;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace NoiseEngine.Nesl.CompilerTools.Parsing;

internal class Parser {

    private readonly List<CompilationError> errors = new List<CompilationError>();

    private List<string>? usings;
    private List<(NeslTypeBuilder, TokenBuffer)>? definedTypes;
    private List<(TypeIdentifierToken typeIdentifier, string name)>? definedFields;
    private List<(
        TypeIdentifierToken typeIdentifier, NameToken name, TokenBuffer parameters, TokenBuffer codeBlock
    )>? definedMethods;
    private List<Parser>? types;
    private List<Parser>? methods;

    private NeslTypeBuilder? currentType;
    private NeslMethodBuilder? currentMethod;
    private Dictionary<string, VariableData>? variables;

    public Parser? Parent { get; }
    public NeslAssemblyBuilder Assembly { get; }
    public string AssemblyPath { get; }
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

    internal IEnumerable<Parser> Types => types ?? Enumerable.Empty<Parser>();

    private NeslTypeBuilder CurrentType {
        get {
            if (currentType is null) {
                string name = Buffer.Tokens[0].Path;
                if (AssemblyPath.Length > 0 && name.StartsWith(AssemblyPath))
                    name = name[AssemblyPath.Length..];
                while (name.StartsWith('/') || name.StartsWith('\\'))
                    name = name[1..];
                while (name.EndsWith('/') || name.EndsWith('\\'))
                    name = name[..^1];
                name = name.Replace('/', '.').Replace('\\', '.');
                currentType = Assembly.DefineType(name);

                string u = currentType.Namespace.Length > 0 ?
                    $"{Assembly.Name}.{currentType.Namespace}" : Assembly.Name;
                if (u.Length > 0)
                    TryDefineUsing(u);
            }
            return currentType;
        }
        init {
            if (value.Namespace.Length > 0)
                TryDefineUsing(value.Namespace);
            currentType = value;
        }
    }

    private IEnumerable<string> Usings {
        get {
            if (usings is not null) {
                lock (usings) {
                    foreach (string u in usings)
                        yield return u;
                }
            }

            if (Parent is not null) {
                foreach (string u in Parent.Usings)
                    yield return u;
            }
        }
    }

    public Parser(
        Parser? parent, NeslAssemblyBuilder assembly, string assemblyPath, ParserStep step, TokenBuffer buffer
    ) {
        Parent = parent;
        Assembly = assembly;
        AssemblyPath = assemblyPath;
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

                    bool result = (bool)(
                        method.Invoke(null, parseParameters) ?? throw new NullReferenceException()
                    );
                    parseParameters[2] = null;
                    if (result) {
                        Buffer.Index = index;
                        _ = Buffer.TryReadNext(TokenType.None, out Token token);
                        errors.Add(new CompilationError(token, CompilationErrorType.UnexpectedExpression));
                        continue;
                    }

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

    public string GetNamespaceFromFilePath(string path) {
        string fullName = Path.GetDirectoryName(path) ?? "";
        if (AssemblyPath.Length > 0 && fullName.StartsWith(AssemblyPath))
            fullName = fullName[AssemblyPath.Length..];
        while (fullName.StartsWith('/') || fullName.StartsWith('\\'))
            fullName = fullName[1..];
        while (fullName.EndsWith('/') || fullName.EndsWith('\\'))
            fullName = fullName[..^1];
        fullName = fullName.Replace('/', '.').Replace('\\', '.');

        if (fullName.Length == 0)
            return Assembly.Name;
        return $"{Assembly.Name}.{fullName}";
    }

    public void AnalyzeTypes() {
        if (definedTypes is null)
            return;

        types = new List<Parser>();
        foreach ((NeslTypeBuilder typeBuilder, TokenBuffer buffer) in definedTypes) {
            Parser parser = new Parser(this, Assembly, AssemblyPath, ParserStep.Type, buffer) {
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
        if (definedFields is not null) {
            foreach ((TypeIdentifierToken typeIdentifier, string name) in definedFields) {
                if (!TryGetType(typeIdentifier, out NeslType? fieldType))
                    continue;

                CurrentType.DefineField(name, fieldType);
            }

            definedFields = null;
        }

        // Add default constructor to value type.
        if (CurrentType.IsValueType) {
            IlGenerator il = CurrentType.DefineMethod(NeslOperators.Constructor, CurrentType).IlGenerator;
            il.Emit(OpCode.DefVariable, CurrentType);
            il.Emit(OpCode.ReturnValue, il.GetNextVariableId());
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
                    this, Assembly, AssemblyPath, ParserStep.Parameters, newParameters
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

                Dictionary<string, VariableData> variables = new Dictionary<string, VariableData>();
                foreach ((NeslType type, string vname) in parameterParser.DefinedParameters)
                    variables.Add(vname, new VariableData(type, vname, method.IlGenerator.GetNextVariableId()));

                methods ??= new List<Parser>();
                methods.Add(new Parser(this, Assembly, AssemblyPath, ParserStep.Method, codeBlock) {
                    CurrentType = CurrentType,
                    CurrentMethod = method!,
                    variables = variables
                });
            }

            definedMethods = null;
        }
    }

    public void AnalyzeMethodBodies() {
        if (methods is not null) {
            foreach (Parser methodParser in methods) {
                methodParser.Parse();
                errors.AddRange(methodParser.Errors);
            }

            methods = null;
        }
    }

    public void Throw(CompilationError error) {
        errors.Add(error);
    }

    public bool TryDefineUsing(string u) {
        usings ??= new List<string>();
        lock (usings) {
            foreach (string u2 in Usings) {
                if (u == u2)
                    return false;
            }
            usings.Add(u);
        }
        return true;
    }

    public void DefineType(NeslTypeBuilder typeBuilder, TokenBuffer buffer) {
        definedTypes ??= new List<(NeslTypeBuilder, TokenBuffer)>();
        definedTypes.Add((typeBuilder, buffer));
    }

    public bool TryDefineField(TypeIdentifierToken typeIdentifier, string name) {
        definedFields ??= new List<(TypeIdentifierToken typeIdentifier, string name)>();
        foreach ((_, string n) in definedFields) {
            if (name == n)
                return false;
        }
        definedFields.Add((typeIdentifier, name));
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

    public bool TryGetMethods(
        TypeIdentifierToken identifier, out bool findedType, [NotNullWhen(true)] out IEnumerable<NeslMethod>? methods
    ) {
        int index = identifier.Identifier.LastIndexOf('.');
        if (index == -1) {
            findedType = true;
            methods = CurrentType.Methods.Where(x => x.Name == identifier.Identifier);
            return methods.Any();
        }

        TypeIdentifierToken i = identifier with { Identifier = identifier.Identifier[..index] };
        if (!TryGetType(i, out NeslType? type)) {
            findedType = false;
            methods = null;
            return false;
        }

        findedType = true;
        string str = identifier.Identifier[(index + 1)..];
        methods = type.Methods.Where(x => x.Name == str);
        return methods.Any();
    }

    public uint DefineVariable(NeslType type, string name) {
        variables ??= new Dictionary<string, VariableData>();
        uint id = CurrentMethod.IlGenerator.GetNextVariableId();
        if (!variables.TryAdd(name, new VariableData(type, name, id)))
            throw new NotImplementedException(); // TODO: Add error.
        return id;
    }

    public VariableData? GetVariable(string name) {
        if (variables is not null && variables.TryGetValue(name, out VariableData data))
            return data;
        return null;
    }

}
