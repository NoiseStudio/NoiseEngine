using System.Collections.Generic;

namespace NoiseEngine.Nesl;

public abstract class NeslType {

    private const char Delimiter = '.';

    public abstract IEnumerable<NeslMethod> Methods { get; }

    public string FullName { get; }
    public NeslAssembly Assembly { get; }

    public string Name => FullName.Substring(FullName.LastIndexOf(Delimiter));
    public string Namespace => FullName.Substring(0, FullName.LastIndexOf(Delimiter));

    protected NeslType(NeslAssembly assembly, string fullName) {
        Assembly = assembly;
        FullName = fullName;
    }

}
