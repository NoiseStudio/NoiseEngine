using NoiseEngine.Collections;
using NoiseEngine.Nesl.CompilerTools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace NoiseEngine.Nesl.Emit;

public class NeslMethodBuilder : NeslMethod {

    private readonly ConcurrentBag<NeslAttribute> attributes = new ConcurrentBag<NeslAttribute>();
    private readonly ConcurrentBag<NeslAttribute> returnValueAttributes = new ConcurrentBag<NeslAttribute>();
    private readonly List<NeslGenericTypeParameterBuilder> genericTypeParameters =
        new List<NeslGenericTypeParameterBuilder>();
    private readonly ConcurrentDictionary<NeslGenericTypeParameter, IReadOnlyList<NeslType>> typeGenericConstraints =
        new ConcurrentDictionary<NeslGenericTypeParameter, IReadOnlyList<NeslType>>();
    private ConcurrentBag<NeslAttribute>[] parameterAttributes;

    private NeslModifiers modifiers;

    public new NeslTypeBuilder Type => (NeslTypeBuilder)base.Type;

    public IlGenerator IlGenerator { get; }

    public override IEnumerable<NeslAttribute> Attributes => attributes;
    public override IEnumerable<NeslAttribute> ReturnValueAttributes => returnValueAttributes;
    public override IReadOnlyList<IEnumerable<NeslAttribute>> ParameterAttributes => parameterAttributes;
    public override IEnumerable<NeslGenericTypeParameter> GenericTypeParameters => genericTypeParameters;
    public override NeslModifiers Modifiers => modifiers;
    public override IReadOnlyDictionary<NeslGenericTypeParameter, IReadOnlyList<NeslType>> TypeGenericConstraints =>
        typeGenericConstraints;

    internal NeslMethodIdentifier Identifier =>
        new NeslMethodIdentifier(Name, new EquatableReadOnlyList<NeslType>(ParameterTypes));

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
    /// Adds new <see cref="NeslGenericTypeParameterBuilder"/> to this <see cref="NeslMethod"/>.
    /// </summary>
    /// <param name="name">Name of new <see cref="NeslGenericTypeParameterBuilder"/>.</param>
    /// <returns>New <see cref="NeslGenericTypeParameterBuilder"/>.</returns>
    /// <exception cref="ArgumentException">
    /// <see cref="NeslGenericTypeParameter"/> with this <paramref name="name"/> already exists in this type.
    /// </exception>
    public NeslGenericTypeParameterBuilder DefineGenericTypeParameter(string name) {
        lock (genericTypeParameters) {
            if (genericTypeParameters.Any(x => x.Name == name)) {
                throw new ArgumentException(
                    $"{nameof(NeslGenericTypeParameter)} named `{name}` already exists in `{Name}` method.",
                    nameof(name)
                );
            }

            NeslGenericTypeParameterBuilder genericTypeParameter = new NeslGenericTypeParameterBuilder(Assembly, name);
            genericTypeParameters.Add(genericTypeParameter);
            return genericTypeParameter;
        }
    }

    /// <summary>
    /// Sets return type for this <see cref="NeslMethodBuilder"/>.
    /// </summary>
    /// <remarks>
    /// Clears <see cref="ReturnValueAttributes"/>.
    /// </remarks>
    /// <param name="returnType"><see cref="NeslType"/> returned from this <see cref="NeslMethodBuilder"/>.</param>
    public void SetReturnType(NeslType? returnType) {
        ReturnType = returnType;
        returnValueAttributes.Clear();
    }

    /// <summary>
    /// Sets the number and types of parameters for this <see cref="NeslMethodBuilder"/>.
    /// </summary>
    /// <remarks>
    /// Clears <see cref="ParameterAttributes"/>.
    /// </remarks>
    /// <param name="parameterTypes"><see cref="NeslType"/> parameters of this <see cref="NeslMethodBuilder"/>.</param>
    public void SetParameters(params NeslType[] parameterTypes) {
        NeslMethodIdentifier lastIdentifier = Identifier;
        ParameterTypes = parameterTypes;

        int oldLength = parameterAttributes.Length;
        if (parameterTypes.Length != oldLength) {
            Array.Resize(ref parameterAttributes, parameterTypes.Length);

            for (int i = oldLength; i < parameterAttributes.Length; i++)
                parameterAttributes[i] = new ConcurrentBag<NeslAttribute>();
        }

        int max = Math.Min(oldLength, parameterAttributes.Length);
        for (int i = 0; i < max; i++)
            parameterAttributes[i].Clear();

        Type.ReplaceMethodIdentifier(lastIdentifier, this);
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

    /// <summary>
    /// Sets <see cref="NeslModifiers"/> for this <see cref="NeslMethodBuilder"/>.
    /// </summary>
    /// <param name="modifiers">New <see cref="NeslModifiers"/> for this <see cref="NeslMethodBuilder"/>.</param>
    public void SetModifiers(NeslModifiers modifiers) {
        this.modifiers = modifiers;
    }

    /// <summary>
    /// Sets type generic constraints for given <paramref name="typeGenericParameter"/> as
    /// <paramref name="constraints"/>.
    /// </summary>
    /// <param name="typeGenericParameter">
    /// <see cref="NeslGenericTypeParameter"/> belonging to <see cref="Type"/>.
    /// </param>
    /// <param name="constraints">
    /// Constraints of this <see cref="NeslMethod"/> on <paramref name="typeGenericParameter"/>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Given <paramref name="typeGenericParameter"/> is not belonging to <see cref="Type"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// One or more of given <paramref name="constraints"/> is not an interface.
    /// </exception>
    public void SetTypeGenericConstraints(
        NeslGenericTypeParameter typeGenericParameter, IReadOnlyList<NeslType> constraints
    ) {
        if (!Type.GenericTypeParameters.Contains(typeGenericParameter)) {
            throw new ArgumentException(
                "Given type generic parameter is not defined in NESL type of this NESL method.",
                nameof(typeGenericParameter)
            );
        }

        foreach (NeslType constraint in constraints) {
            if (!constraint.IsInterface) {
                throw new ArgumentException(
                    $"Given type `{constraint}` is not an interface.", nameof(constraints)
                );
            }
        }

        typeGenericConstraints[typeGenericParameter] = constraints;
    }

}
