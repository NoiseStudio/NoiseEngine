using System.Collections.Generic;

namespace NoiseEngine.Nesl.Serialization;

internal sealed class SerializedNeslGenericTypeParameter : NeslGenericTypeParameter {

    public override IEnumerable<NeslAttribute> Attributes { get; }

    public SerializedNeslGenericTypeParameter(
        NeslAssembly assembly, string name, IEnumerable<NeslAttribute> attributes
    ) : base(assembly, name) {
        Attributes = attributes;
    }

}
