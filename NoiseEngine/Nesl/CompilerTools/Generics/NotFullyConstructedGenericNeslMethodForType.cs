using System.Collections.Generic;

namespace NoiseEngine.Nesl.CompilerTools.Generics;

internal sealed class NotFullyConstructedGenericNeslMethodForType : NeslMethod {

    public NeslMethod ParentMethod { get; }

    public override IEnumerable<NeslAttribute> Attributes => ParentMethod.Attributes;
    public override IEnumerable<NeslAttribute> ReturnValueAttributes => ParentMethod.ReturnValueAttributes;
    public override IReadOnlyList<IEnumerable<NeslAttribute>> ParameterAttributes => ParentMethod.ParameterAttributes;
    public override IEnumerable<NeslGenericTypeParameter> GenericTypeParameters => ParentMethod.GenericTypeParameters;
    public override NeslModifiers Modifiers => ParentMethod.Modifiers;

    protected override IlContainer IlContainer => ParentMethod.GetIlContainer();

    public NotFullyConstructedGenericNeslMethodForType(
        NotFullyConstructedGenericNeslType type, NeslMethod parentMethod, NeslType? returnType,
        NeslType[] parameterTypes
    ) : base(type, parentMethod.Name, returnType, parameterTypes) {
        ParentMethod = parentMethod;
    }

}
