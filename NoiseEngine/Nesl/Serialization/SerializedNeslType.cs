using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NoiseEngine.Nesl.Serialization;

internal class SerializedNeslType : NeslType {

    private ImmutableDictionary<uint, NeslField>? idToField;

    private ImmutableArray<NeslGenericTypeParameter> genericTypeParameters =
        ImmutableArray.Create<NeslGenericTypeParameter>();
    private ImmutableArray<NeslField> fields = ImmutableArray.Create<NeslField>();
    private ImmutableArray<NeslMethod> methods = ImmutableArray.Create<NeslMethod>();

    public override IEnumerable<NeslAttribute> Attributes { get; }
    public override IEnumerable<NeslGenericTypeParameter> GenericTypeParameters => genericTypeParameters;
    public override IEnumerable<NeslField> Fields => fields;
    public override IEnumerable<NeslMethod> Methods => methods;

    public SerializedNeslType(NeslAssembly assembly, string fullName, ImmutableArray<NeslAttribute> attributes)
        : base(assembly, fullName) {
        Attributes = attributes;
    }

    internal void SetGenericTypeParameters(ImmutableArray<NeslGenericTypeParameter> genericTypeParameters) {
        this.genericTypeParameters = genericTypeParameters;
    }

    internal void SetFields(ImmutableArray<NeslField> fields, ImmutableDictionary<uint, NeslField> idToField) {
        this.fields = fields;
        this.idToField = idToField;
    }

    internal void SetMethods(ImmutableArray<NeslMethod> methods) {
        this.methods = methods;
    }

    internal override NeslField GetField(uint localFieldId) {
        return idToField?[localFieldId] ?? throw new NullReferenceException();
    }

}
