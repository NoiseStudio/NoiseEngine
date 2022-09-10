using System.Collections.Generic;
using System.Linq;

namespace NoiseEngine.Nesl.CompilerTools.Generics;

internal class GenericNeslMethodInGenericMakedNeslType : NeslMethod {

    public NeslMethod ParentMethod { get; }
    public IReadOnlyDictionary<NeslGenericTypeParameter, NeslType> TypeTargetTypes { get; }

    public override IEnumerable<NeslAttribute> Attributes => ParentMethod.Attributes;
    public override IEnumerable<NeslAttribute> ReturnValueAttributes => ParentMethod.ReturnValueAttributes;
    public override IReadOnlyList<IEnumerable<NeslAttribute>> ParameterAttributes => ParentMethod.ParameterAttributes;
    public override IEnumerable<NeslGenericTypeParameter> GenericTypeParameters => ParentMethod.GenericTypeParameters;

    protected override IlContainer IlContainer => ParentMethod.GetIlContainer();

    public GenericNeslMethodInGenericMakedNeslType(
        NeslType type, NeslMethod parentMethod, IReadOnlyDictionary<NeslGenericTypeParameter, NeslType> typeTargetTypes
    ) : base(type, parentMethod.Name, parentMethod.ReturnType, parentMethod.ParameterTypes.ToArray()) {
        ParentMethod = parentMethod;
        TypeTargetTypes = typeTargetTypes;
    }

    public override NeslMethod MakeGeneric(params NeslType[] typeArguments) {
        return MakeGenericWorker(typeArguments, TypeTargetTypes);
    }

}
