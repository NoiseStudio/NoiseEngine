using NoiseEngine.Nesl.Serialization;
using NoiseEngine.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace NoiseEngine.Nesl.CompilerTools.Generics;

internal class NotFullyConstructedGenericNeslType : NeslType {

    public NeslType ParentType { get; }
    public ImmutableArray<NeslType> TypeArguments { get; }

    public override IEnumerable<NeslAttribute> Attributes => ParentType.Attributes;
    public override IEnumerable<NeslGenericTypeParameter> GenericTypeParameters => ParentType.GenericTypeParameters;
    public override IReadOnlyList<NeslField> Fields => ParentType.Fields;
    public override IEnumerable<NeslMethod> Methods => ParentType.Methods;

    public NotFullyConstructedGenericNeslType(NeslType parentType, ImmutableArray<NeslType> typeArguments) : base(
        parentType.Assembly, parentType.FullName
    ) {
        ParentType = parentType;
        TypeArguments = typeArguments;
    }

    public override NeslType MakeGeneric(params NeslType[] typeArguments) {
        return ParentType.MakeGeneric(typeArguments);
    }

    internal override void PrepareHeader(SerializationUsed used, NeslAssembly serializedAssembly) {
        used.Add(this, ParentType);
        used.Add(this, TypeArguments);
    }

    internal override bool SerializeHeader(NeslAssembly serializedAssembly, SerializationWriter writer) {
        Debug.Assert(serializedAssembly == Assembly);

        writer.WriteBool(true);
        writer.WriteUInt8((byte)NeslTypeUsageKind.GenericNotFullyConstructed);
        writer.WriteUInt64(Assembly.GetLocalTypeId(ParentType));
        writer.WriteEnumerable(TypeArguments.Select(Assembly.GetLocalTypeId));
        return false;
    }

}
