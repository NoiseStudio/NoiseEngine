using NoiseEngine.Nesl.Emit.Attributes;
using System.Collections.Generic;

namespace NoiseEngine.Nesl;

public abstract class NeslField {

    public abstract IEnumerable<NeslAttribute> Attributes { get; }

    public NeslType ParentType { get; }
    public string Name { get; }
    public NeslType FieldType { get; }

    public bool IsStatic => Attributes.HasAnyAttribute(nameof(StaticAttribute));

    protected NeslField(NeslType parentType, string name, NeslType fieldType) {
        ParentType = parentType;
        Name = name;
        FieldType = fieldType;
    }

}
