using System;
using System.Collections.Generic;

namespace NoiseEngine.Nesl;

public abstract class NeslGenericTypeParameter : NeslType {

    public override IEnumerable<NeslGenericTypeParameter> GenericTypeParameters => throw NewStillGenericException();
    public override IEnumerable<NeslField> Fields => throw NewStillGenericException();
    public override IEnumerable<NeslMethod> Methods => throw NewStillGenericException();

    public INeslGenericTypeParameterOwner Owner { get; }

    public override string Name => FullName;
    public override string Namespace => string.Empty;

    protected NeslGenericTypeParameter(INeslGenericTypeParameterOwner owner, string name) : base(owner.Assembly, name) {
        Owner = owner;
    }

    internal void AssertConstraints(NeslType type) {
    }

    internal void AssertUsedOwner(INeslGenericTypeParameterOwner usedOwner) {
        if (Owner != usedOwner)
            throw new InvalidOperationException($"{nameof(NeslGenericTypeParameter)} has been used out of scope.");
    }

    internal override NeslField GetField(uint localFieldId) {
        throw NewStillGenericException();
    }

    private Exception NewStillGenericException() {
        return new InvalidOperationException(
            $"This type is {nameof(NeslGenericTypeParameter)}. " +
            "Construct final type by invoking MakeGeneric method on owner before use."
        );
    }

}
