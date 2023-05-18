using System.Collections.Generic;

namespace NoiseEngine.Nesl.Serialization;

internal class SerializedNeslAssembly : NeslAssembly {

    private NeslType[] types = null!;

    public override IEnumerable<NeslAssembly> Dependencies { get; }
    public override IEnumerable<NeslType> Types => types;

    public SerializedNeslAssembly(
        string name, IEnumerable<NeslAssembly> dependencies
    ) : base(name) {
        Dependencies = dependencies;
    }

    public void SetTypes(NeslType[] types) {
        this.types = types;
    }

}
