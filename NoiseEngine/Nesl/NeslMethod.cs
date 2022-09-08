using NoiseEngine.Nesl.CompilerTools;
using NoiseEngine.Nesl.Emit.Attributes;
using System;
using System.Collections.Generic;

namespace NoiseEngine.Nesl;

public abstract class NeslMethod {

    public abstract IEnumerable<NeslAttribute> Attributes { get; }
    public abstract IEnumerable<NeslAttribute> ReturnValueAttributes { get; }
    public abstract IReadOnlyList<IEnumerable<NeslAttribute>> ParameterAttributes { get; }

    protected abstract IlContainer IlContainer { get; }

    public NeslType Type { get; }
    public string Name { get; }
    public NeslType? ReturnType { get; }
    public IReadOnlyList<NeslType> ParameterTypes { get; }
    public Guid Guid { get; }

    public NeslAssembly Assembly => Type.Assembly;
    public string FullName => $"{Type.FullName}::{Name}";

    public bool IsStatic => Attributes.HasAnyAttribute(nameof(StaticAttribute));

    protected NeslMethod(NeslType type, string name, NeslType? returnType, NeslType[] parameterTypes) {
        Type = type;
        Name = name;
        ReturnType = returnType;
        ParameterTypes = parameterTypes;

        Guid = Guid.NewGuid();
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() {
        return FullName;
    }

    internal IEnumerable<Instruction> GetInstructions() {
        return IlContainer.GetInstructions();
    }

    internal IlContainer GetIlContainer() {
        return IlContainer;
    }

}
