using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NoiseEngine.Nesl.Serialization;

internal class SerializedNeslType : NeslType {

    private ImmutableDictionary<uint, NeslField>? idToField;

    private NeslGenericTypeParameter[] genericTypeParameters = Array.Empty<NeslGenericTypeParameter>();
    private NeslField[] fields = Array.Empty<NeslField>();
    private NeslMethod[] methods = Array.Empty<NeslMethod>();

    public override IEnumerable<NeslAttribute> Attributes { get; }
    public override IEnumerable<NeslGenericTypeParameter> GenericTypeParameters => genericTypeParameters;
    public override IEnumerable<NeslField> Fields => fields;
    public override IEnumerable<NeslMethod> Methods => methods;

    public SerializedNeslType(NeslAssembly assembly, string fullName, NeslAttribute[] attributes)
        : base(assembly, fullName) {
        Attributes = attributes;
    }

    internal void SetGenericTypeParameters(NeslGenericTypeParameter[] genericTypeParameters) {
        this.genericTypeParameters = genericTypeParameters;
    }

    internal void SetFields(NeslField[] fields, ImmutableDictionary<uint, NeslField> idToField) {
        this.fields = fields;
        this.idToField = idToField;
    }

    internal void SetMethods(NeslMethod[] methods) {
        this.methods = methods;
    }

    internal override NeslField GetField(uint localFieldId) {
        return idToField?[localFieldId] ?? throw new NullReferenceException();
    }

}
