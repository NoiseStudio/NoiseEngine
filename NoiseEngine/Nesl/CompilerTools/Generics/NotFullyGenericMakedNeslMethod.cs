using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NoiseEngine.Nesl.CompilerTools.Generics;

internal class NotFullyGenericMakedNeslMethod : NeslMethod {

    public NeslMethod ParentMethod { get; }
    public ImmutableArray<NeslType> TypeArguments { get; }

    public override IEnumerable<NeslAttribute> Attributes => ParentMethod.Attributes;
    public override IEnumerable<NeslAttribute> ReturnValueAttributes => ParentMethod.ReturnValueAttributes;
    public override IReadOnlyList<IEnumerable<NeslAttribute>> ParameterAttributes => ParentMethod.ParameterAttributes;
    public override IEnumerable<NeslGenericTypeParameter> GenericTypeParameters => ParentMethod.GenericTypeParameters;

    protected override IlContainer IlContainer => ParentMethod.GetIlContainer();

    public NotFullyGenericMakedNeslMethod(NeslMethod parentMethod, ImmutableArray<NeslType> typeArguments) : base(
        parentMethod.Type, parentMethod.Name, parentMethod.ReturnType, Array.Empty<NeslType>()
    ) {
        ParentMethod = parentMethod;
        TypeArguments = typeArguments;
    }

    public override NeslMethod MakeGeneric(params NeslType[] typeArguments) {
        return ParentMethod.MakeGeneric(typeArguments);
    }

}
