using NoiseEngine.Nesl.CompilerTools.Generics;
using NoiseEngine.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NoiseEngine.Nesl.Serialization;

internal class SerializedNeslType : NeslType, IGenericMakedForInitialize {

    private NeslAttribute[] attributes = Array.Empty<NeslAttribute>();
    private NeslType[] genericMakedTypeParameters = Array.Empty<NeslType>();
    private NeslGenericTypeParameter[] genericTypeParameters = Array.Empty<NeslGenericTypeParameter>();
    private NeslField[] fields = Array.Empty<NeslField>();
    private IReadOnlyList<NeslMethod> methods = Array.Empty<NeslMethod>();
    private NeslType[] interfaces = Array.Empty<NeslType>();

    public override IEnumerable<NeslAttribute> Attributes => attributes;
    public override IEnumerable<NeslType> GenericMakedTypeParameters => genericMakedTypeParameters;
    public override IEnumerable<NeslGenericTypeParameter> GenericTypeParameters => genericTypeParameters;
    public override IReadOnlyList<NeslField> Fields => fields;
    public override IEnumerable<NeslMethod> Methods => methods;
    public override NeslTypeKind Kind { get; }
    public override IEnumerable<NeslType> Interfaces => interfaces;

    public SerializedNeslType(NeslAssembly assembly, SerializationReader reader) : this(
        assembly, (NeslTypeKind)reader.ReadUInt8(), reader.ReadString(),
        reader.ReadEnumerable<NeslAttribute>().ToArray(), null, Array.Empty<NeslType>()
    ) {
    }

    public SerializedNeslType(
        NeslAssembly assembly, NeslTypeKind kind, string fullName, NeslAttribute[] attributes,
        NeslType? genericMakedFrom, NeslType[] genericMakedTypeParameters
    ) : base(assembly, fullName, genericMakedFrom) {
        Kind = kind;
        this.attributes = attributes;
        this.genericMakedTypeParameters = genericMakedTypeParameters;
    }

    internal void DeserializeBody(SerializationReader reader) {
        SetGenericTypeParameters(reader.ReadEnumerableUInt64().Select(Assembly.GetType)
            .Cast<NeslGenericTypeParameter>().ToArray()
        );

        NeslField[] fields = new NeslField[reader.ReadInt32()];
        for (int i = 0; i < fields.Length; i++)
            fields[i] = NeslField.Deserialize(reader);
        SetFields(fields);

        NeslType[] interfaces = new NeslType[reader.ReadInt32()];
        for (int i = 0; i < interfaces.Length; i++)
            interfaces[i] = Assembly.GetType(reader.ReadUInt64());
        SetInterfaces(interfaces);
    }

    internal void DeserializeMethods(SerializationReader reader) {
        NeslMethod[] methods = new NeslMethod[reader.ReadInt32()];
        for (int i = 0; i < methods.Length; i++)
            methods[i] = NeslMethod.Deserialize(reader);
        SetMethods(methods);
    }

    internal void SetAttributes(NeslAttribute[] attributes) {
        this.attributes = attributes;
    }

    internal void SetGenericMakedTypeParameters(NeslType[] genericMakedTypeParameters) {
        this.genericMakedTypeParameters = genericMakedTypeParameters;
    }

    internal void SetGenericTypeParameters(NeslGenericTypeParameter[] genericTypeParameters) {
        this.genericTypeParameters = genericTypeParameters;
    }

    internal void SetFields(NeslField[] fields) {
        this.fields = fields;
    }

    internal void SetInterfaces(NeslType[] interfaces) {
        this.interfaces = interfaces;
    }

    internal void SetMethods(IReadOnlyList<NeslMethod> methods) {
        this.methods = methods;
    }

    internal List<(NeslMethod, NeslMethod)> UnsafeInitializeTypeFromMakeGeneric(
        IReadOnlyDictionary<NeslGenericTypeParameter, NeslType> targetTypes
    ) {
        if (GenericMakedFrom is null)
            throw new InvalidOperationException("This type is not generic maked.");

        // Interfaces.
        IReadOnlyDictionary<NeslType, IReadOnlyList<NeslConstraint>>? forConstraints = GenericMakedFrom.ForConstraints;
        if (forConstraints is not null) {
            List<NeslType> interfaces = new List<NeslType>();
            foreach (NeslType i in GenericMakedFrom.Interfaces) {
                if (forConstraints.TryGetValue(i, out IReadOnlyList<NeslConstraint>? constraints)) {
                    bool isSatisfied = true;

                    foreach (NeslConstraint constraint in constraints) {
                        NeslType finalType = targetTypes[constraint.GenericTypeParameter];
                        foreach (NeslType constraintType in constraint.Constraints) {
                            if (!finalType.Interfaces.Contains(GenericHelper.GetFinalType(
                                GenericMakedFrom, this, constraintType, targetTypes!
                            ))) {
                                isSatisfied = false;
                                break;
                            }
                        }
                    }

                    if (!isSatisfied)
                        continue;
                }

                interfaces.Add(GenericHelper.GetFinalType(GenericMakedFrom, this, i, targetTypes!));
            }

            SetInterfaces(interfaces.ToArray());
        } else {
            SetInterfaces(GenericMakedFrom.Interfaces.Select(x => GenericHelper.GetFinalType(
                GenericMakedFrom, this, x, targetTypes!
            )).ToArray());
        }

        // Attributes.
        SetAttributes(GenericHelper.RemoveGenericsFromAttributes(GenericMakedFrom.Attributes, targetTypes));

        // Create fields.
        List<NeslField> fields = new List<NeslField>();
        foreach (NeslField field in GenericMakedFrom.Fields) {
            fields.Add(new SerializedNeslField(
                this, field.Name, GenericHelper.GetFinalType(GenericMakedFrom, this, field.FieldType, targetTypes),
                GenericHelper.RemoveGenericsFromAttributes(field.Attributes, targetTypes),
                field.DefaultData?.ToArray()
            ));
        }

        SetFields(fields.ToArray());

        // Create methods.
        List<(NeslMethod, NeslMethod)> methods = new List<(NeslMethod, NeslMethod)>();
        foreach (NeslMethod method in GenericMakedFrom.Methods) {
            if (method.IsGeneric) {
                methods.Add((method, new GenericNeslMethodInConstructedGenericNeslType(this, method, targetTypes)));
                continue;
            }

            // Check type constraints.
            bool constraintsSatisfied = true;
            foreach (
                (NeslGenericTypeParameter parameter, IReadOnlyList<NeslType> constraints) in
                method.TypeGenericConstraints
            ) {
                NeslType type = GenericHelper.GetFinalType(GenericMakedFrom, this, parameter, targetTypes);
                foreach (NeslType constraint in constraints) {
                    NeslType finalConstraint = GenericHelper.GetFinalType(
                        GenericMakedFrom, this, constraint, targetTypes
                    );

                    if (!type.Interfaces.Contains(finalConstraint)) {
                        constraintsSatisfied = false;
                        break;
                    }
                }

                if (!constraintsSatisfied)
                    break;
            }

            if (!constraintsSatisfied)
                continue;

            // Return and parameter types.
            NeslType? methodReturnType = method.ReturnType;
            if (methodReturnType is not null)
                methodReturnType = GenericHelper.GetFinalType(GenericMakedFrom, this, methodReturnType, targetTypes);

            NeslType[] methodParameterTypes = new NeslType[method.ParameterTypes.Count];

            int i = 0;
            foreach (NeslType parameterType in method.ParameterTypes) {
                methodParameterTypes[i++] = GenericHelper.GetFinalType(
                    GenericMakedFrom, this, parameterType, targetTypes
                );
            }

            // Construct new method.
            methods.Add((method, new SerializedNeslMethod(
                method.Modifiers,
                this,
                method.Name,
                methodReturnType,
                methodParameterTypes,
                GenericHelper.RemoveGenericsFromAttributes(method.Attributes, targetTypes),
                GenericHelper.RemoveGenericsFromAttributes(method.ReturnValueAttributes, targetTypes),
                method.ParameterAttributes.Select(x => GenericHelper.RemoveGenericsFromAttributes(x, targetTypes)),
                method.GenericTypeParameters.ToArray(),
                ImmutableDictionary<NeslGenericTypeParameter, IReadOnlyList<NeslType>>.Empty,
                null
            )));
        }

        SetMethods(methods.Select(x => x.Item2).ToArray());
        return methods;
    }

    internal void UnsafeInitializeTypeFromMakeGenericMethodIl(
        List<(NeslMethod, NeslMethod)> methods, IReadOnlyDictionary<NeslGenericTypeParameter, NeslType> targetTypes
    ) {
        foreach ((NeslMethod original, NeslMethod newMethod) in methods) {
            if (newMethod is not SerializedNeslMethod serialized)
                continue;

            serialized.SetIlContainer(GenericIlGenerator.RemoveGenerics(
                GenericMakedFrom!, this, original, targetTypes
            ));
        }
    }

}
