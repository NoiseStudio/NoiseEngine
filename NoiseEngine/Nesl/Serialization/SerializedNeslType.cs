﻿using NoiseEngine.Nesl.CompilerTools.Generics;
using NoiseEngine.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NoiseEngine.Nesl.Serialization;

internal class SerializedNeslType : NeslType {

    private IReadOnlyDictionary<NeslGenericTypeParameter, NeslType>? targetTypes;
    private NeslAttribute[] attributes = Array.Empty<NeslAttribute>();
    private NeslType[] genericMakedTypeParameters = Array.Empty<NeslType>();
    private NeslGenericTypeParameter[] genericTypeParameters = Array.Empty<NeslGenericTypeParameter>();
    private NeslField[] fields = Array.Empty<NeslField>();
    private NeslMethod[] methods = Array.Empty<NeslMethod>();
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

    internal void SetMethods(NeslMethod[] methods) {
        this.methods = methods;
    }

    internal void SetGenericTargetTypes(IReadOnlyDictionary<NeslGenericTypeParameter, NeslType> targetTypes) {
        this.targetTypes = targetTypes;
    }

    internal void UnsafeInitializeTypeFromMakeGeneric() {
        if (targetTypes is null)
            return;

        lock (attributes) {
            if (targetTypes is null)
                return;

            UnsafeInitializeTypeFromMakeGenericWorker();
            targetTypes = null;
        }
    }

    internal void UnsafeInitializeTypeFromMakeGenericWorker() {
        if (GenericMakedFrom is null)
            throw new InvalidOperationException("This type is not generic maked.");
        if (targetTypes is null)
            throw new UnreachableException();

        SetInterfaces(GenericMakedFrom.Interfaces.Select(x => GenericHelper.GetFinalType(
            GenericMakedFrom, this, x, targetTypes!
        )).ToArray());
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
        List<NeslMethod> methods = new List<NeslMethod>();

        foreach (NeslMethod method in GenericMakedFrom.Methods) {
            if (method.IsGeneric) {
                methods.Add(new GenericNeslMethodInConstructedGenericNeslType(this, method, targetTypes));
                continue;
            }

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
            methods.Add(new SerializedNeslMethod(
                method.Modifiers,
                this,
                method.Name,
                methodReturnType,
                methodParameterTypes,
                GenericHelper.RemoveGenericsFromAttributes(method.Attributes, targetTypes),
                GenericHelper.RemoveGenericsFromAttributes(method.ReturnValueAttributes, targetTypes),
                method.ParameterAttributes.Select(x => GenericHelper.RemoveGenericsFromAttributes(x, targetTypes)),
                method.GenericTypeParameters.ToArray(),
                GenericIlGenerator.RemoveGenerics(GenericMakedFrom, this, method, targetTypes)
            ));
        }

        SetMethods(methods.ToArray());
    }

}
