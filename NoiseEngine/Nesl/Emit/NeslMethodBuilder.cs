using NoiseEngine.Nesl.CompilerTools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.Emit;

public class NeslMethodBuilder : NeslMethod {

    private readonly ConcurrentBag<NeslAttribute> attributes = new ConcurrentBag<NeslAttribute>();
    private readonly ConcurrentBag<NeslAttribute> returnValueAttributes = new ConcurrentBag<NeslAttribute>();
    private readonly ConcurrentBag<NeslAttribute>[] parameterAttributes;

    public IlGenerator IlGenerator { get; }

    public override IEnumerable<NeslAttribute> Attributes => attributes;
    public override IEnumerable<NeslAttribute> ReturnValueAttributes => returnValueAttributes;
    public override IReadOnlyList<IEnumerable<NeslAttribute>> ParameterAttributes => parameterAttributes;

    protected override IlContainer IlContainer => IlGenerator;

    internal NeslMethodBuilder(NeslTypeBuilder type, string name, NeslType? returnType, NeslType[] parameterTypes)
        : base(type, name, returnType, parameterTypes)
    {
        IlGenerator = new IlGenerator((NeslAssemblyBuilder)type.Assembly, this);

        parameterAttributes = new ConcurrentBag<NeslAttribute>[parameterTypes.Length];
        for (int i = 0; i < parameterAttributes.Length; i++)
            parameterAttributes[i] = new ConcurrentBag<NeslAttribute>();
    }

    /// <summary>
    /// Adds <paramref name="attribute"/> to this <see cref="NeslMethodBuilder"/>.
    /// </summary>
    /// <param name="attribute"><see cref="NeslAttribute"/>.</param>
    /// <exception cref="InvalidOperationException">
    /// Given <paramref name="attribute"/> cannot be assigned to this target.
    /// </exception>
    public void AddAttribute(NeslAttribute attribute) {
        if (!attribute.Targets.HasFlag(AttributeTargets.Method)) {
            throw new InvalidOperationException(
                $"The `{attribute}` attribute cannot be assigned to a method.");
        }

        attributes.Add(attribute);
    }

    /// <summary>
    /// Adds <paramref name="attribute"/> to this <see cref="NeslMethodBuilder"/> return value.
    /// </summary>
    /// <param name="attribute"><see cref="NeslAttribute"/>.</param>
    /// <exception cref="InvalidOperationException">
    /// Given <paramref name="attribute"/> cannot be assigned to this target.
    /// </exception>
    public void AddAttributeToReturnValue(NeslAttribute attribute) {
        if (!attribute.Targets.HasFlag(AttributeTargets.ReturnValue)) {
            throw new InvalidOperationException(
                $"The `{attribute}` attribute cannot be assigned to a return value.");
        }

        returnValueAttributes.Add(attribute);
    }

    /// <summary>
    /// Adds <paramref name="attribute"/> to this <see cref="NeslMethodBuilder"/>'s
    /// parameter at given <paramref name="parameterIndex"/>.
    /// </summary>
    /// <param name="parameterIndex">Index of parameter.</param>
    /// <param name="attribute"><see cref="NeslAttribute"/>.</param>
    /// <exception cref="InvalidOperationException">
    /// Given <paramref name="attribute"/> cannot be assigned to this target.
    /// </exception>
    public void AddAttributeToParameter(int parameterIndex, NeslAttribute attribute) {
        if (!attribute.Targets.HasFlag(AttributeTargets.Parameter)) {
            throw new InvalidOperationException(
                $"The `{attribute}` attribute cannot be assigned to a parameter.");
        }

        parameterAttributes[parameterIndex].Add(attribute);
    }

}
