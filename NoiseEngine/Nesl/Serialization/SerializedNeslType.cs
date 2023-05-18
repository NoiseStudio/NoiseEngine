using NoiseEngine.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NoiseEngine.Nesl.Serialization;

internal class SerializedNeslType : NeslType {

    private NeslGenericTypeParameter[] genericTypeParameters = Array.Empty<NeslGenericTypeParameter>();
    private NeslField[] fields = Array.Empty<NeslField>();
    private NeslMethod[] methods = Array.Empty<NeslMethod>();

    public override IEnumerable<NeslAttribute> Attributes { get; }
    public override IEnumerable<NeslType> GenericMakedTypeParameters { get; }
    public override IEnumerable<NeslGenericTypeParameter> GenericTypeParameters => genericTypeParameters;
    public override IReadOnlyList<NeslField> Fields => fields;
    public override IEnumerable<NeslMethod> Methods => methods;

    public SerializedNeslType(NeslAssembly assembly, string fullName, NeslAttribute[] attributes) : this(
        assembly, fullName, attributes, Array.Empty<NeslType>()
    ) {
    }

    public SerializedNeslType(
        NeslAssembly assembly, string fullName, NeslAttribute[] attributes, NeslType[] genericMakedTypeParameters
    ) : base(assembly, fullName) {
        Attributes = attributes;
        GenericMakedTypeParameters = genericMakedTypeParameters;
    }

    internal void DeserializeBody(SerializationReader reader) {
        SetGenericTypeParameters(reader.ReadEnumerableUInt64().Select(Assembly.GetType)
            .Cast<NeslGenericTypeParameter>().ToArray()
        );
        SetFields(reader.ReadEnumerable<NeslField>().ToArray());
        SetMethods(reader.ReadEnumerable<NeslMethod>().ToArray());
    }

    internal void SetGenericTypeParameters(NeslGenericTypeParameter[] genericTypeParameters) {
        this.genericTypeParameters = genericTypeParameters;
    }

    internal void SetFields(NeslField[] fields) {
        this.fields = fields;
    }

    internal void SetMethods(NeslMethod[] methods) {
        this.methods = methods;
    }

}
