using NoiseEngine.Nesl.Serialization;
using NoiseEngine.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NoiseEngine.Nesl;

public abstract class NeslGenericTypeParameter : NeslType {

    public override IEnumerable<NeslGenericTypeParameter> GenericTypeParameters => throw NewStillGenericException();
    public override IReadOnlyList<NeslField> Fields => throw NewStillGenericException();
    public override IEnumerable<NeslMethod> Methods => throw NewStillGenericException();

    public override string Name => FullName;
    public override string Namespace => string.Empty;

    protected NeslGenericTypeParameter(NeslAssembly assembly, string name) : base(assembly, name) {
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

    internal override void PrepareHeader(SerializationUsed used, NeslAssembly serializedAssembly) {
        used.Register(this);
    }

    internal override bool SerializeHeader(NeslAssembly serializedAssembly, SerializationWriter writer) {
        Debug.Assert(serializedAssembly == Assembly);

        writer.WriteBool(true);
        writer.WriteUInt8((byte)NeslTypeUsageKind.GenericTypeParameter);
        writer.WriteString(FullName);
        writer.WriteEnumerable(Attributes);

        Debug.Assert(!IsGenericMaked);
        return false;
    }

}
