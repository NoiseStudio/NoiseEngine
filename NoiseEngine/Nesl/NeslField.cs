using NoiseEngine.Nesl.Emit.Attributes;
using System.Collections.Generic;
using System.Diagnostics;

namespace NoiseEngine.Nesl;

public abstract class NeslField {

    public abstract IEnumerable<NeslAttribute> Attributes { get; }
    public abstract IReadOnlyList<byte>? DefaultData { get; }

    public NeslType ParentType { get; }
    public string Name { get; }
    public NeslType FieldType { get; }

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

}
