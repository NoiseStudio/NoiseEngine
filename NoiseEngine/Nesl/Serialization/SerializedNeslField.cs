using System.Collections.Generic;
using System.Collections.Immutable;

namespace NoiseEngine.Nesl.Serialization;

internal class SerializedNeslField : NeslField {

    public override IEnumerable<NeslAttribute> Attributes { get; }

    public SerializedNeslField(
        NeslType parentType, string name, NeslType fieldType, ImmutableArray<NeslAttribute> attributes
    ) : base(parentType, name, fieldType) {
        Attributes = attributes;
    }

}
