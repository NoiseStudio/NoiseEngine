using System.Collections.Generic;

namespace NoiseEngine.Nesl.CompilerTools.Generics;

internal class NotFullyConstructedGenericNeslField : NeslField {

    private readonly NeslField genericMaked;

    public override IEnumerable<NeslAttribute> Attributes => genericMaked.Attributes;
    public override IReadOnlyList<byte>? DefaultData => genericMaked.DefaultData;

    public NotFullyConstructedGenericNeslField(
        NotFullyConstructedGenericNeslType parentType, NeslField genericMaked, NeslType fieldType
    ) : base(parentType, genericMaked.Name, fieldType) {
        this.genericMaked = genericMaked;
    }

}
