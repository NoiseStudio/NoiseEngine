using NoiseEngine.Nesl.Serialization;
using NoiseEngine.Serialization;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace NoiseEngine.Nesl.CompilerTools.Generics;

internal sealed class NotFullyConstructedGenericNeslType : NeslType {

    public override IEnumerable<NeslAttribute> Attributes => GenericMakedFrom!.Attributes;
    public override IEnumerable<NeslGenericTypeParameter> GenericTypeParameters =>
        GenericMakedFrom!.GenericTypeParameters;
    public override IReadOnlyList<NeslField> Fields { get; }
    public override IEnumerable<NeslMethod> Methods { get; }
    public override NeslType? GenericMakedFrom { get; }
    public override NeslTypeKind Kind => GenericMakedFrom!.Kind;
    public override IEnumerable<NeslType> Interfaces => GenericMakedFrom!.Interfaces;
    public override IEnumerable<NeslType> GenericMakedTypeParameters { get; }

    public NotFullyConstructedGenericNeslType(
        NeslType parentType, Dictionary<NeslGenericTypeParameter, NeslType> targetTypes,
        ImmutableArray<NeslType> genericMakedTypeParameters
    ) : base(
        parentType.Assembly, parentType.FullName
    ) {
        GenericMakedFrom = parentType;
        GenericMakedTypeParameters = genericMakedTypeParameters;

        Fields = GenericMakedFrom.Fields.Select(field => new NotFullyConstructedGenericNeslField(
            this, field, GenericHelper.GetFinalType(GenericMakedFrom, this, field.FieldType, targetTypes)
        )).ToArray();

        Methods = GenericMakedFrom.Methods.Select(method => new NotFullyConstructedGenericNeslMethodForType(
            this, method,
            method.ReturnType is null ? null : GenericHelper.GetFinalType(
                GenericMakedFrom, this, method.ReturnType, targetTypes
            ),
            method.ParameterTypes.Select(x => GenericHelper.GetFinalType(
                GenericMakedFrom, this, x, targetTypes
            )).ToArray()
        )).ToArray();
    }

    public override NeslType MakeGeneric(params NeslType[] typeArguments) {
        return GenericMakedFrom!.MakeGeneric(typeArguments);
    }

    internal override void PrepareHeader(SerializationUsed used, NeslAssembly serializedAssembly) {
        used.Add(this, GenericMakedFrom!);
    }

    internal override bool SerializeHeader(NeslAssembly serializedAssembly, SerializationWriter writer) {
        Debug.Assert(serializedAssembly == Assembly);

        writer.WriteBool(true);
        writer.WriteUInt8((byte)NeslTypeUsageKind.GenericMaked);
        writer.WriteUInt64(Assembly.GetLocalTypeId(GenericMakedFrom!));
        return true;
    }

}
