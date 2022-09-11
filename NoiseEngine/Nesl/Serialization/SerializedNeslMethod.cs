using NoiseEngine.Nesl.CompilerTools;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NoiseEngine.Nesl.Serialization;

internal class SerializedNeslMethod : NeslMethod {

    public override IEnumerable<NeslAttribute> Attributes { get; }
    public override IEnumerable<NeslAttribute> ReturnValueAttributes { get; }
    public override IReadOnlyList<IEnumerable<NeslAttribute>> ParameterAttributes { get; }
    public override IEnumerable<NeslGenericTypeParameter> GenericTypeParameters { get; }

    protected override IlContainer IlContainer { get; }

    public SerializedNeslMethod(
        NeslType type, string name, NeslType? returnType, NeslType[] parameterTypes,
        NeslAttribute[] attributes, NeslAttribute[] returnValueAttributes,
        IEnumerable<IEnumerable<NeslAttribute>> parameterAttributes,
        NeslGenericTypeParameter[] genericTypeParameters, IlContainer ilContainer
    ) : base(type, name, returnType, parameterTypes) {
        Attributes = attributes;
        ReturnValueAttributes = returnValueAttributes;
        ParameterAttributes = parameterAttributes.ToArray();
        GenericTypeParameters = genericTypeParameters;
        IlContainer = ilContainer;
    }

}
