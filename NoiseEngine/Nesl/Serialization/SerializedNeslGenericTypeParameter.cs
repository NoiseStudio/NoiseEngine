using System.Collections.Generic;

namespace NoiseEngine.Nesl.Serialization;

internal sealed class SerializedNeslGenericTypeParameter : NeslGenericTypeParameter {

    public override IEnumerable<NeslAttribute> Attributes { get; }
    public override IEnumerable<NeslType> Interfaces { get; }
    public override IEnumerable<NeslMethod> Methods { get; }

    public SerializedNeslGenericTypeParameter(
        NeslAssembly assembly, string name, IEnumerable<NeslAttribute> attributes, IEnumerable<NeslType> interfaces
    ) : base(assembly, name) {
        Attributes = attributes;
        Interfaces = interfaces;
        Methods = CreateMethodsFromInterfaces();
    }

}
