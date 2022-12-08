using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NoiseEngine.Nesl.CompilerTools.Generics;

internal class NotFullyConstructedGenericNeslType : NeslType {

    public NeslType ParentType { get; }
    public ImmutableArray<NeslType> TypeArguments { get; }

    public override IEnumerable<NeslAttribute> Attributes => ParentType.Attributes;
    public override IEnumerable<NeslGenericTypeParameter> GenericTypeParameters => ParentType.GenericTypeParameters;
    public override IReadOnlyList<NeslField> Fields => ParentType.Fields;
    public override IEnumerable<NeslMethod> Methods => ParentType.Methods;

    public NotFullyConstructedGenericNeslType(NeslType parentType, ImmutableArray<NeslType> typeArguments) : base(
        parentType.Assembly, parentType.FullName
    ) {
        ParentType = parentType;
        TypeArguments = typeArguments;
    }

    public override NeslType MakeGeneric(params NeslType[] typeArguments) {
        return ParentType.MakeGeneric(typeArguments);
    }

    internal override NeslField GetField(uint localFieldId) {
        throw new InvalidOperationException(
            "This type is not fully constructed generic type. " +
            "Construct final type by invoking MakeGeneric method and use the return type."
        );
    }

}
