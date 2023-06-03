using NoiseEngine.Nesl.CompilerTools;
using System.Collections.Generic;
using System.Linq;

namespace NoiseEngine.Nesl.Serialization;

internal class SerializedNeslMethod : NeslMethod {

    public override IEnumerable<NeslAttribute> Attributes { get; }
    public override IEnumerable<NeslAttribute> ReturnValueAttributes { get; }
    public override IReadOnlyList<IEnumerable<NeslAttribute>> ParameterAttributes { get; }
    public override IEnumerable<NeslGenericTypeParameter> GenericTypeParameters { get; }
    public override NeslModifiers Modifiers { get; }
    public override IReadOnlyDictionary<
        NeslGenericTypeParameter, IReadOnlyList<NeslType>
    > TypeGenericConstraints { get; }

    protected override IlContainer IlContainer { get; }

    public SerializedNeslMethod(
        NeslModifiers modifiers, NeslType type, string name, NeslType? returnType, NeslType[] parameterTypes,
        NeslAttribute[] attributes, NeslAttribute[] returnValueAttributes,
        IEnumerable<IEnumerable<NeslAttribute>> parameterAttributes,
        NeslGenericTypeParameter[] genericTypeParameters,
        IReadOnlyDictionary<NeslGenericTypeParameter, IReadOnlyList<NeslType>> typeGenericConstraints,
        IlContainer ilContainer
    ) : base(type, name, returnType, parameterTypes) {
        Modifiers = modifiers;
        Attributes = attributes;
        ReturnValueAttributes = returnValueAttributes;
        ParameterAttributes = parameterAttributes.ToArray();
        GenericTypeParameters = genericTypeParameters;
        TypeGenericConstraints = typeGenericConstraints;
        IlContainer = ilContainer;
    }

}
