using NoiseEngine.Nesl.Emit.Attributes;
using System.Collections.Generic;

namespace NoiseEngine.Nesl;

public abstract class NeslType {

    private const char Delimiter = '.';

    public abstract IEnumerable<NeslAttribute> Attributes { get; }
    public abstract IEnumerable<NeslField> Fields { get; }
    public abstract IEnumerable<NeslMethod> Methods { get; }

    public NeslAssembly Assembly { get; }
    public string FullName { get; }

    public string Name => FullName.Substring(FullName.LastIndexOf(Delimiter) + 1);
    public string Namespace => FullName.Substring(0, FullName.LastIndexOf(Delimiter));

    public bool IsClass => !IsValueType;
    public bool IsValueType => Attributes.HasAnyAttribute(nameof(ValueTypeAttribute));

    protected NeslType(NeslAssembly assembly, string fullName) {
        Assembly = assembly;
        FullName = fullName;
    }

    internal abstract NeslField GetField(uint localFieldId);

}
