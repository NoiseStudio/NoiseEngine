using System.Collections.Generic;

namespace NoiseEngine.Nesl.Serialization;

internal sealed class SerializedNeslGenericTypeParameter : NeslGenericTypeParameter {

    public override IEnumerable<NeslAttribute> Attributes { get; }

    public SerializedNeslGenericTypeParameter(
        INeslGenericTypeParameterOwner owner, string name, IEnumerable<NeslAttribute> attributes
    ) : base(owner, name) {
        Attributes = attributes;
    }

}
