namespace NoiseEngine.Nesl;

public abstract class NeslField {

    public NeslType ParentType { get; }
    public string Name { get; }
    public NeslType FieldType { get; }

    protected NeslField(NeslType parentType, string name, NeslType fieldType) {
        ParentType = parentType;
        Name = name;
        FieldType = fieldType;
    }

}
