using System.Collections.Generic;

namespace NoiseEngine.Nesl;

public abstract class NeslAssembly {

    public abstract IEnumerable<NeslType> Types { get; }

    public string Name { get; }

    protected NeslAssembly(string name) {
        Name = name;
    }

    internal abstract NeslType GetType(ulong localTypeId);
    internal abstract NeslMethod GetMethod(ulong localMethodId);

}
