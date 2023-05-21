using NoiseEngine.Nesl.Emit.Attributes;
using NoiseEngine.Nesl.Serialization;
using NoiseEngine.Serialization;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NoiseEngine.Nesl;

public abstract class NeslField {

    public abstract IEnumerable<NeslAttribute> Attributes { get; }
    public abstract IReadOnlyList<byte>? DefaultData { get; }

    public NeslType ParentType { get; }
    public string Name { get; }
    public NeslType FieldType { get; }

    public NeslAssembly Assembly => ParentType.Assembly;
    public bool IsStatic => Attributes.HasAnyAttribute(nameof(StaticAttribute));

    internal uint Id {
        get {
            uint id = 0;
            foreach (NeslField field in ParentType.Fields) {
                if (field == this)
                    return id;
                id++;
            }

            throw new UnreachableException();
        }
    }

    protected NeslField(NeslType parentType, string name, NeslType fieldType) {
        ParentType = parentType;
        Name = name;
        FieldType = fieldType;
    }

    /// <summary>
    /// Creates new <see cref="NeslField"/> with data from <paramref name="reader"/>.
    /// </summary>
    /// <param name="reader"><see cref="SerializationReader"/>.</param>
    /// <returns>New <see cref="NeslField"/> with data from <paramref name="reader"/>.</returns>
    internal static NeslField Deserialize(SerializationReader reader) {
        NeslAssembly assembly = reader.GetFromStorage<NeslAssembly>();
        return new SerializedNeslField(
            assembly.GetType(reader.ReadUInt64()),
            reader.ReadString(),
            assembly.GetType(reader.ReadUInt64()),
            reader.ReadEnumerable<NeslAttribute>().ToArray(),
            reader.ReadBool() ? reader.ReadEnumerableUInt8().ToArray() : null
        );
    }

    internal void Serialize(SerializationUsed used, SerializationWriter writer) {
        writer.WriteUInt64(Assembly.GetLocalTypeId(ParentType));
        writer.WriteString(Name);
        used.Add(ParentType, FieldType);
        writer.WriteUInt64(Assembly.GetLocalTypeId(FieldType));

        writer.WriteEnumerable(Attributes);

        if (DefaultData is null) {
            writer.WriteBool(false);
        } else {
            writer.WriteBool(true);
            writer.WriteEnumerable(DefaultData);
        }
    }

}
