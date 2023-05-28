using NoiseEngine.Nesl.Serialization;
using NoiseEngine.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace NoiseEngine.Nesl;

public abstract class NeslGenericTypeParameter : NeslType {

    private static int nextInstanceId;

    private readonly int instanceId;

    public override IEnumerable<NeslGenericTypeParameter> GenericTypeParameters => throw NewStillGenericException();
    public override IReadOnlyList<NeslField> Fields => throw NewStillGenericException();
    public override IEnumerable<NeslMethod> Methods => Interfaces.SelectMany(x => x.Methods);
    public override NeslTypeKind Kind => NeslTypeKind.GenericParameter;

    public override string Name => FullName;
    public override string Namespace => string.Empty;

    protected NeslGenericTypeParameter(NeslAssembly assembly, string name) : base(assembly, name) {
        instanceId = Interlocked.Increment(ref nextInstanceId);
    }

    public override string ToString() {
        return $"{base.ToString()} {{ InstanceId = {instanceId} }}";
    }

    private static Exception NewStillGenericException() {
        return new InvalidOperationException(
            $"This type is {nameof(NeslGenericTypeParameter)}. " +
            "Construct final type by invoking MakeGeneric method on owner and use the return type."
        );
    }

    internal void AssertConstraints(NeslType type) {
        bool isSatisfied = true;
        foreach (NeslType constraint in Interfaces) {
            if (!type.Interfaces.Contains(constraint)) {
                isSatisfied = false;
                break;
            }
        }

        if (!isSatisfied)
            throw new ArgumentException("Type does not satisfy constraints.", nameof(type));
    }

    internal override void PrepareHeader(SerializationUsed used, NeslAssembly serializedAssembly) {
        used.Register(this);
        used.Add(this, Interfaces);
    }

    internal override bool SerializeHeader(NeslAssembly serializedAssembly, SerializationWriter writer) {
        Debug.Assert(serializedAssembly == Assembly);

        writer.WriteBool(true);
        writer.WriteUInt8((byte)NeslTypeUsageKind.GenericTypeParameter);
        writer.WriteString(FullName);
        writer.WriteEnumerable(Attributes);
        writer.WriteEnumerable(Interfaces.Select(Assembly.GetLocalTypeId));

        Debug.Assert(!IsGenericMaked);
        return false;
    }

}
