using System.Collections.Generic;

namespace NoiseEngine.Nesl.Serialization;

internal class SerializedNeslField : NeslField {

    public override IEnumerable<NeslAttribute> Attributes { get; }
    public override IReadOnlyList<byte>? DefaultData { get; }

    public SerializedNeslField(
        NeslType parentType, string name, NeslType fieldType, NeslAttribute[] attributes, byte[]? defaultData
    ) : base(parentType, name, fieldType) {
        Attributes = attributes;
        DefaultData = defaultData;
    }

}
