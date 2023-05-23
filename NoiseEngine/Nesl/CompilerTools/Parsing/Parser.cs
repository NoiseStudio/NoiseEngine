using NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;
using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;
using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.Emit.Attributes.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NoiseEngine.Nesl.CompilerTools.Parsing;

internal class Parser {

    private readonly List<CompilationError> errors = new List<CompilationError>();

    private List<string>? usings;
    private List<(NeslTypeBuilder, TokenBuffer)>? definedTypes;
    private List<(TypeIdentifierToken typeIdentifier, string name)>? definedFields;
    private List<PropertyDefinitionData>? definedProperties;
    private List<MethodDefinitionData>? definedMethods;
    private List<Parser>? types;
    private List<Parser>? methods;

    private NeslTypeBuilder? currentType;
    private NeslMethodBuilder? currentMethod;
    private Dictionary<string, VariableData>? variables;
    private bool replacedDefaultConstructor;

    public Parser? Parent { get; }
    public NeslAssemblyBuilder Assembly { get; }
    public string AssemblyPath { get; }
    public ParserStep Step { get; }
    public TokenBuffer Buffer { get; }
    public IReadOnlyList<CompilationError> Errors => errors;
    public CompilationErrorMode ErrorMode { get; } = new CompilationErrorMode();
    public IEnumerable<Parser> Types => types ?? Enumerable.Empty<Parser>();
    public uint InstanceVariableId => (uint)CurrentMethod.Type.Fields.Count + (uint)CurrentMethod.ParameterTypes.Count;

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

    public bool CurrentMethodIsConstructor {
        get => (currentMethod ?? throw new UnreachableException()).Name.StartsWith(NeslOperators.Constructor);
    }

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

        AnalyzeProperties();

        // Add default constructor to value type.
        if (currentType?.IsValueType == true) {
            IlGenerator il = CurrentType.DefineMethod(NeslOperators.Constructor, CurrentType).IlGenerator;
            il.Emit(OpCode.DefVariable, CurrentType);
            il.Emit(OpCode.ReturnValue, il.GetNextVariableId());
        }
    }

    public void AnalyzeMethods() {
        if (definedMethods is not null) {
            foreach (MethodDefinitionData data in definedMethods) {
                // Parameters.
                TokenBuffer newParameters;
                if (data.Parameters.Tokens.Count == 0) {
                    newParameters = data.Parameters;
                } else {
                    Token last = data.Parameters.Tokens[^1];
                    newParameters = new TokenBuffer(data.Parameters.Tokens.Append(new Token(
                        last.Path, last.Line, last.Column + 1, TokenType.Comma, 1, null
                    )).ToArray());
                }

                ParameterParser parameterParser = new ParameterParser(
                    this, Assembly, AssemblyPath, ParserStep.Parameters, newParameters
                ) {
                    CurrentType = CurrentType
                    // TODO: Add generic parameters from this method.
                };
                parameterParser.Parse();

                errors.AddRange(parameterParser.Errors);

                // Return type.
                NeslType? returnType = null;
                if (data.IsConstructor)
                    returnType = currentType ?? throw new UnreachableException();
                else if (data.TypeIdentifier!.Value.Identifier != "void")
                    TryGetType(data.TypeIdentifier.Value, out returnType);

                // Replace default constructor.
                if (
                    data.IsConstructor && parameterParser.DefinedParameters.Count == 0 &&
                    !replacedDefaultConstructor && CurrentType.IsValueType
                ) {
                    replacedDefaultConstructor = true;
                    NeslMethodBuilder defaultConstructor = (NeslMethodBuilder)(
                        CurrentType.GetMethod(data.Name.Name) ?? throw new UnreachableException()
                    );
                    CurrentType.RemoveMethod(defaultConstructor);
                }

                // Construct.
                if (!CurrentType.TryDefineMethod(
                    data.Name.Name, out NeslMethodBuilder? method, returnType,
                    parameterParser.DefinedParameters.Select(x => x.type).ToArray()
                )) {
                    Throw(new CompilationError(
                        data.Name.Pointer, CompilationErrorType.MethodAlreadyExists, data.Name.Name
                    ));

                    // Create method with random name.
                    while (!CurrentType.TryDefineMethod(
                        $"{data.Name.Name}-{Random.Shared.NextInt64()}", out method, returnType,
                        parameterParser.DefinedParameters.Select(x => x.type).ToArray()
                    )) { }
                }

                foreach (NeslAttribute attribute in data.Attributes)
                    method.AddAttribute(attribute);

                // Ignore body when has intrinsic attribute.
                if (method.Attributes.HasAnyAttribute(IntrinsicAttribute.Create().FullName))
                    continue;

                Dictionary<string, VariableData> variables = new Dictionary<string, VariableData>();
                uint index = (uint)method.Type.Fields.Count;
                foreach ((NeslType type, string vname) in parameterParser.DefinedParameters)
                    variables.Add(vname, new VariableData(type, vname, index++));

                if (data.IsConstructor)
                    DefaultConstructorHelper.AppendHeader(method);

                methods ??= new List<Parser>();
                methods.Add(new Parser(this, Assembly, AssemblyPath, ParserStep.Method, data.CodeBlock) {
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

                if (methodParser.CurrentMethodIsConstructor)
                    DefaultConstructorHelper.AppendFooter(methodParser.CurrentMethod);

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
        if (definedFields is not null) {
            foreach ((_, string n) in definedFields) {
                if (name == n)
                    return false;
            }
        }
        if (definedProperties is not null) {
            foreach (string n in definedProperties.Select(x => x.Name.Name)) {
                if (name == n)
                    return false;
            }
        }

        definedFields ??= new List<(TypeIdentifierToken typeIdentifier, string name)>();
        definedFields.Add((typeIdentifier, name));
        return true;
    }

    public bool TryDefineProperty(PropertyDefinitionData data) {
        if (definedProperties is not null) {
            foreach (string n in definedProperties.Select(x => x.Name.Name)) {
                if (data.Name.Name == n)
                    return false;
            }
        }
        if (definedFields is not null) {
            foreach ((_, string n) in definedFields) {
                if (data.Name.Name == n)
                    return false;
            }
        }

        definedProperties ??= new List<PropertyDefinitionData>();
        definedProperties.Add(data);
        return true;
    }

    public void DefineMethod(MethodDefinitionData data) {
        definedMethods ??= new List<MethodDefinitionData>();
        definedMethods.Add(data);
    }

    public bool TryGetType(TypeIdentifierToken typeIdentifier, [NotNullWhen(true)] out NeslType? type) {
        string name = typeIdentifier.Identifier;
        if (typeIdentifier.GenericTokens.Count > 0)
            name += $"`{typeIdentifier.GenericTokens.Count}";

        if (currentMethod is not null) {
            type = currentMethod.GenericTypeParameters.FirstOrDefault(x => x.Name == name);
            if (type is not null)
                return true;
        }

        if (currentType is not null) {
            type = currentType.GenericTypeParameters.FirstOrDefault(x => x.Name == name);
            if (type is not null)
                return true;
        }

        type = typeIdentifier.GetTypeFromAssembly(
            Assembly, Usings, out IReadOnlyList<TypeIdentifierToken> genericTokens
        );
        if (type is not null) {
            Debug.Assert(type.GenericTypeParameters.Count() == genericTokens.Count);
            if (genericTokens.Count == 0)
                return true;

            NeslType[] genericTypes = new NeslType[genericTokens.Count];
            for (int i = 0; i < genericTypes.Length; i++) {
                if (!TryGetType(genericTokens[i], out NeslType? genericType))
                    return false;
                genericTypes[i] = genericType;
            }

            type = type.MakeGeneric(genericTypes);
            return true;
        }

        type = null;
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

    private void AnalyzeProperties() {
        if (definedProperties is null)
            return;

        foreach (PropertyDefinitionData data in definedProperties) {
            DefineMethod(new MethodDefinitionData(
                data.TypeIdentifier, data.Name with { Name = NeslOperators.PropertyGet + data.Name.Name },
                TokenBuffer.Empty, TokenBuffer.Empty, data.GetterAttributes
            ));

            if (!data.HasSetter && !data.HasInitializer)
                continue;

            string s = NeslOperators.PropertySet;
            if (data.HasInitializer)
                s = NeslOperators.PropertyInit;

            DefineMethod(new MethodDefinitionData(
                data.TypeIdentifier, data.Name with { Name = s + data.Name.Name }, TokenBuffer.Empty, TokenBuffer.Empty,
                data.SecondAttributes
            ));
        }
    }

}
