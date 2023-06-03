using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NoiseEngine.Nesl.CompilerTools.Generics;

internal sealed class NeslGenericTypeParameterImplementedMethod : NeslMethod {

    public NeslMethod ParentMethod { get; }

    public override IEnumerable<NeslAttribute> Attributes => ParentMethod.Attributes;
    public override IEnumerable<NeslAttribute> ReturnValueAttributes => ParentMethod.ReturnValueAttributes;
    public override IReadOnlyList<IEnumerable<NeslAttribute>> ParameterAttributes => ParentMethod.ParameterAttributes;
    public override IEnumerable<NeslGenericTypeParameter> GenericTypeParameters => ParentMethod.GenericTypeParameters;
    public override NeslModifiers Modifiers => ParentMethod.Modifiers;
    public override IReadOnlyDictionary<NeslGenericTypeParameter, IReadOnlyList<NeslType>> TypeGenericConstraints =>
        ParentMethod.TypeGenericConstraints;

    protected override IlContainer IlContainer => throw new UnreachableException();

    public NeslGenericTypeParameterImplementedMethod(
        NeslGenericTypeParameter type, NeslMethod parentMethod
    ) : base(type, parentMethod.Name, parentMethod.ReturnType, parentMethod.ParameterTypes.ToArray()) {
        ParentMethod = parentMethod;
    }

}
