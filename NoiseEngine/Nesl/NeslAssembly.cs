using System.Collections.Generic;

namespace NoiseEngine.Nesl;

public abstract class NeslAssembly {

    public abstract IEnumerable<NeslType> Types { get; }

    public NeslAssemblyName? Name { get; }

    protected NeslAssembly(NeslAssemblyName? name) {
        Name = name;
    }

}
