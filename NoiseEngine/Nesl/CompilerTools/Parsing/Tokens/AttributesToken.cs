using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

internal readonly record struct AttributesToken(
    IReadOnlyList<AttributeToken> Attributes
) : IParserToken<AttributesToken> {

    public bool IsIgnored => this == default;
    public int Priority => 0;

    public static bool Parse(
        TokenBuffer buffer, CompilationErrorMode errorMode, [NotNullWhen(true)] out AttributesToken result,
        out CompilationError error
    ) {
        int index = buffer.Index;
        List<AttributeToken> attributes = new List<AttributeToken>();
        while (AttributeToken.Parse(buffer, errorMode, out AttributeToken attribute, out _)) {
            attributes.Add(attribute);
            index = buffer.Index;
        }

        buffer.Index = index;
        if (buffer.TryReadNext(TokenType.SquareBracketOpening, out Token token)) {
            result = default;
            error = new CompilationError(token, CompilationErrorType.ExpectedAttribute);
            return false;
        }
        buffer.Index = index;

        result = new AttributesToken(attributes);
        error = default;
        return true;
    }

    public IReadOnlyList<NeslAttribute> Compile(Parser parser, AttributeTargets target) {
        List<NeslAttribute>? result = null;
        List<ConstValueToken>? parameters = null;

        foreach (AttributeToken attribute in Attributes) {
            string name = $"{attribute.Identifier.Identifier}Attribute";
            Type? attributeType = typeof(Parser).Assembly.DefinedTypes.FirstOrDefault(x => x.Name == name)?.AsType();
            if (attributeType is null) {
                parser.Throw(new CompilationError(
                    attribute.Pointer, CompilationErrorType.AttributeNotFound, attribute.Identifier.Identifier
                ));
                continue;
            }

            if (attribute.Parameters is not null) {
                TokenBuffer? buffer = attribute.Parameters;
                while (buffer.HasNextTokens) {
                    if (!ConstValueToken.Parse(
                        buffer, parser.ErrorMode, out ConstValueToken value, out CompilationError error
                    )) {
                        parser.Throw(error);
                    } else {
                        parameters ??= new List<ConstValueToken>();
                        parameters.Add(value);
                    }

                    if (buffer.HasNextTokens && !buffer.TryReadNext(TokenType.Comma, out Token token))
                        parser.Throw(new CompilationError(token, CompilationErrorType.ExpectedComma));
                }
            }

            bool found = false;
            int bestMatch = -1;
            CompilationError? finalError = null;

            foreach (
                MethodInfo m in attributeType.GetMethods(BindingFlags.Static | BindingFlags.Public)
                    .Where(x => x.Name == "Create")
            ) {
                ParameterInfo[] p = m.GetParameters();
                if (p.Length != (parameters?.Count ?? 0))
                    continue;

                bool match = true;
                object[] args = new object[p.Length];
                for (int i = 0; i < p.Length; i++) {
                    Type type = p[i].ParameterType;
                    ConstValueToken v = parameters![i];

                    bool r = false;
                    CompilationError error = default;
                    if (type == typeof(long)) {
                        r = v.ToInt64(out long a, out error);
                        args[i] = a;
                    } else if (type == typeof(int)) {
                        r = v.ToInt32(out int a, out error);
                        args[i] = a;
                    } else if (type == typeof(short)) {
                        r = v.ToInt16(out short a, out error);
                        args[i] = a;
                    } else if (type == typeof(sbyte)) {
                        r = v.ToInt8(out sbyte a, out error);
                        args[i] = a;
                    } else if (type == typeof(ulong)) {
                        r = v.ToUInt64(out ulong a, out error);
                        args[i] = a;
                    } else if (type == typeof(uint)) {
                        r = v.ToUInt32(out uint a, out error);
                        args[i] = a;
                    } else if (type == typeof(ushort)) {
                        r = v.ToUInt16(out ushort a, out error);
                        args[i] = a;
                    } else if (type == typeof(byte)) {
                        r = v.ToUInt8(out byte a, out error);
                        args[i] = a;
                    } else if (type == typeof(double)) {
                        r = v.ToFloat64(out double a, out error);
                        args[i] = a;
                    } else if (type == typeof(float)) {
                        r = v.ToFloat32(out float a, out error);
                        args[i] = a;
                    } else if (type == typeof(Half)) {
                        r = v.ToFloat16(out Half a, out error);
                        args[i] = a;
                    } else if (type == typeof(bool)) {
                        r = v.ToBool(out bool a, out error);
                        args[i] = a;
                    } else if (type == typeof(string)) {
                        r = v.ToString(out string a, out error);
                        args[i] = a;
                    }

                    if (!r) {
                        if (i > bestMatch) {
                            bestMatch = i;
                            finalError = error;
                        }
                        match = false;
                        break;
                    }
                }

                if (match) {
                    found = true;
                    NeslAttribute a = (NeslAttribute)(m.Invoke(null, args) ?? throw new NullReferenceException());
                    if (!a.Targets.HasFlag(target)) {
                        parser.Throw(new CompilationError(
                            attribute.Pointer, CompilationErrorType.AttributeTargetNotMatch,
                            attribute.Identifier.Identifier
                        ));
                        break;
                    }

                    result ??= new List<NeslAttribute>();
                    result.Add(a);
                    break;
                }
            }

            if (!found) {
                if (finalError.HasValue) {
                    parser.Throw(finalError.Value);
                } else {
                    parser.Throw(new CompilationError(
                        attribute.Pointer, CompilationErrorType.AttributeCreateMethodNotFound,
                        attribute.Identifier.Identifier
                    ));
                }
            }

            parameters?.Clear();
        }

        return result is null ? Array.Empty<NeslAttribute>() : result;
    }

}
