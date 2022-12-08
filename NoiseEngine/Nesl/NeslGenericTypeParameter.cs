using System;
using System.Collections.Generic;

namespace NoiseEngine.Nesl;

public abstract class NeslGenericTypeParameter : NeslType {

    public override IEnumerable<NeslGenericTypeParameter> GenericTypeParameters => throw NewStillGenericException();
    public override IReadOnlyList<NeslField> Fields => throw NewStillGenericException();
    public override IEnumerable<NeslMethod> Methods => throw NewStillGenericException();

    public INeslGenericTypeParameterOwner Owner { get; }

    public override string Name => FullName;
    public override string Namespace => string.Empty;

    protected NeslGenericTypeParameter(INeslGenericTypeParameterOwner owner, string name) : base(owner.Assembly, name) {
        Owner = owner;
    }

    private static Exception NewStillGenericException() {
        return new InvalidOperationException(
            $"This type is {nameof(NeslGenericTypeParameter)}. " +
            "Construct final type by invoking MakeGeneric method on owner and use the return type."
        );
    }

    internal void AssertConstraints(NeslType type) {
        // TODO: add constraints.
    }

    internal override NeslField GetField(uint localFieldId) {
        throw NewStillGenericException();
    }

}
