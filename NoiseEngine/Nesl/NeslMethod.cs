using NoiseEngine.Nesl.Runtime;
using System.Collections.Generic;

namespace NoiseEngine.Nesl;

public abstract class NeslMethod {

    public abstract IEnumerable<NeslAttribute> Attributes { get; }

    protected abstract IlContainer IlContainer { get; }

    public string Name { get; }
    public NeslType Type { get; }

    protected NeslMethod(NeslType type, string name) {
        Type = type;
        Name = name;
    }

    internal IEnumerable<Instruction> GetInstructions() {
        return IlContainer.GetInstructions();
    }

}
