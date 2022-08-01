using System.Collections.Generic;

namespace NoiseEngine.Nesl;

public abstract class NeslType {

    private const char Delimiter = '.';

    public abstract IEnumerable<NeslAttribute> CustomAttributes { get; }
    public abstract IEnumerable<NeslMethod> Methods { get; }

    public NeslAssembly Assembly { get; }
    public string FullName { get; }
    public TypeAttributes Attributes { get; }

    public string Name => FullName.Substring(FullName.LastIndexOf(Delimiter));
    public string Namespace => FullName.Substring(0, FullName.LastIndexOf(Delimiter));

    protected NeslType(NeslAssembly assembly, string fullName, TypeAttributes attributes) {
        Assembly = assembly;
        FullName = fullName;
        Attributes = attributes;
    }

}
