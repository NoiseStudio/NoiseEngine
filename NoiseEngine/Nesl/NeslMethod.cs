using NoiseEngine.Nesl.CompilerTools;
using System.Collections.Generic;

namespace NoiseEngine.Nesl;

public abstract class NeslMethod {

    public abstract IEnumerable<NeslAttribute> CustomAttributes { get; }

    protected abstract IlContainer IlContainer { get; }

    public NeslType Type { get; }
    public string Name { get; }
    public MethodAttributes Attributes { get; }
    public NeslType? ReturnType { get; }
    public IReadOnlyList<NeslType> ParameterTypes { get; }

    protected NeslMethod(
        NeslType type, string name, MethodAttributes attributes, NeslType? returnType, NeslType[] parameterTypes
    ) {
        Type = type;
        Name = name;
        Attributes = attributes;
        ReturnType = returnType;
        ParameterTypes = parameterTypes;
    }

    internal IEnumerable<Instruction> GetInstructions() {
        return IlContainer.GetInstructions();
    }

}
