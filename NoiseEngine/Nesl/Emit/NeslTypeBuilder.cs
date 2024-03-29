﻿using NoiseEngine.Collections.Concurrent;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace NoiseEngine.Nesl.Emit;

public class NeslTypeBuilder : NeslType {

    private readonly ConcurrentBag<NeslAttribute> attributes = new ConcurrentBag<NeslAttribute>();
    private readonly List<NeslGenericTypeParameterBuilder> genericTypeParameters =
        new List<NeslGenericTypeParameterBuilder>();
    private readonly List<NeslFieldBuilder> fields =
        new List<NeslFieldBuilder>();
    private readonly ConcurrentDictionary<NeslMethodIdentifier, NeslMethodBuilder> methods =
        new ConcurrentDictionary<NeslMethodIdentifier, NeslMethodBuilder>();
    private readonly ConcurrentHashSet<NeslType> interfaces = new ConcurrentHashSet<NeslType>();

    private ConcurrentDictionary<NeslType, IReadOnlyList<NeslConstraint>>? forConstraints;

    private NeslTypeKind kind = NeslTypeKind.Struct;

    public override IEnumerable<NeslAttribute> Attributes => attributes;
    public override IEnumerable<NeslGenericTypeParameter> GenericTypeParameters => genericTypeParameters;
    public override IReadOnlyList<NeslField> Fields => fields;
    public override IEnumerable<NeslMethod> Methods => methods.Values;
    public override NeslTypeKind Kind => kind;
    public override IEnumerable<NeslType> Interfaces =>
        interfaces.Concat(interfaces.SelectMany(x => x.Interfaces)).Distinct();

    protected internal override IReadOnlyDictionary<NeslType, IReadOnlyList<NeslConstraint>>? ForConstraints =>
        forConstraints;

    internal NeslTypeBuilder(NeslAssemblyBuilder assembly, string fullName) : base(assembly, fullName) {
    }

    /// <summary>
    /// Adds new <see cref="NeslGenericTypeParameterBuilder"/> to this <see cref="NeslType"/>.
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
                    $"{nameof(NeslGenericTypeParameter)} named `{name}` already exists in `{Name}` type.",
                    nameof(name)
                );
            }

            NeslGenericTypeParameterBuilder genericTypeParameter = new NeslGenericTypeParameterBuilder(Assembly, name);
            genericTypeParameters.Add(genericTypeParameter);

            return genericTypeParameter;
        }
    }

    /// <summary>
    /// Creates new <see cref="NeslFieldBuilder"/> in this type.
    /// </summary>
    /// <param name="name">Name of new <see cref="NeslFieldBuilder"/>.</param>
    /// <param name="fieldType"><see cref="NeslType"/> of new field.</param>
    /// <returns>New <see cref="NeslFieldBuilder"/>.</returns>
    /// <exception cref="ArgumentException">
    /// <see cref="NeslField"/> with this <paramref name="name"/> already exists in this type.
    /// </exception>
    public NeslFieldBuilder DefineField(string name, NeslType fieldType) {
        NeslFieldBuilder field = new NeslFieldBuilder(this, name, fieldType);

        lock (fields) {
            if (fields.Any(x => x.Name == name)) {
                throw new ArgumentException($"{nameof(NeslField)} named `{name}` already exists in `{Name}` type.",
                    nameof(name));
            }
            fields.Add(field);
        }

        return field;
    }

    /// <summary>
    /// Creates new <see cref="NeslMethodBuilder"/> in this type.
    /// </summary>
    /// <param name="name">Name of new <see cref="NeslMethodBuilder"/>.</param>
    /// <param name="returnType"><see cref="NeslType"/> returned from new method.</param>
    /// <param name="parameterTypes"><see cref="NeslType"/> parameters of new method.</param>
    /// <returns>New <see cref="NeslMethodBuilder"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// <see cref="NeslMethod"/> with this <paramref name="name"/> and
    /// <paramref name="parameterTypes"/> already exists in this type.
    /// </exception>
    public NeslMethodBuilder DefineMethod(string name, NeslType? returnType = null, params NeslType[] parameterTypes) {
        NeslMethodBuilder method = new NeslMethodBuilder(this, name, returnType, parameterTypes);
        AddMethodToCollection(method);
        return method;
    }

    /// <summary>
    /// Tries to create new <see cref="NeslMethodBuilder"/> in this type.
    /// </summary>
    /// <param name="name">Name of new <see cref="NeslMethodBuilder"/>.</param>
    /// <param name="method">
    /// New <see cref="NeslMethodBuilder"/> or <see langword="null"/> when result is <see langword="false"/>.
    /// </param>
    /// <param name="returnType"><see cref="NeslType"/> returned from new method.</param>
    /// <param name="parameterTypes"><see cref="NeslType"/> parameters of new method.</param>
    /// <returns><see langword="true"/> when type is successfuly defined; otherwise <see langword="false"/>.</returns>
    public bool TryDefineMethod(
        string name, [NotNullWhen(true)] out NeslMethodBuilder? method, NeslType? returnType = null,
        params NeslType[] parameterTypes
    ) {
        method = new NeslMethodBuilder(this, name, returnType, parameterTypes);
        if (TryAddMethodToCollection(method))
            return true;

        method = null;
        return false;
    }

    /// <summary>
    /// Adds <paramref name="attribute"/> to this <see cref="NeslTypeBuilder"/>.
    /// </summary>
    /// <param name="attribute"><see cref="NeslAttribute"/>.</param>
    /// <exception cref="InvalidOperationException">
    /// Given <paramref name="attribute"/> cannot be assigned to this target.
    /// </exception>
    public void AddAttribute(NeslAttribute attribute) {
        if (!attribute.Targets.HasFlag(AttributeTargets.Type)) {
            throw new InvalidOperationException(
                $"The `{attribute}` attribute cannot be assigned to a type.");
        }

        attributes.Add(attribute);
    }

    /// <summary>
    /// Sets <see cref="NeslTypeKind"/> of this <see cref="NeslTypeBuilder"/>.
    /// </summary>
    /// <param name="kind">New <see cref="NeslTypeKind"/> of this <see cref="NeslTypeBuilder"/>.</param>
    public void SetKind(NeslTypeKind kind) {
        this.kind = kind;
    }

    /// <summary>
    /// Implements given <paramref name="interfaceType"/> in this <see cref="NeslTypeBuilder"/>.
    /// </summary>
    /// <param name="interfaceType">Interface which this <see cref="NeslTypeBuilder"/> implements.</param>
    /// <param name="constraints"><paramref name="interfaceType"/> for constraints.</param>
    /// <exception cref="ArgumentException">Given <paramref name="interfaceType"/> is not an interface.</exception>
    public void AddInterface(NeslType interfaceType, IEnumerable<NeslConstraint>? constraints = null) {
        if (!interfaceType.IsInterface)
            throw new ArgumentException("Given type is not an interface.", nameof(interfaceType));
        interfaces.Add(interfaceType);

        if (constraints is null || !constraints.Any())
            return;

        if (forConstraints is null) {
            Interlocked.CompareExchange(
                ref forConstraints, new ConcurrentDictionary<NeslType, IReadOnlyList<NeslConstraint>>(), null
            );
        }

        foreach (NeslType constraint in constraints.SelectMany(x => x.Constraints)) {
            if (!constraint.IsInterface)
                throw new ArgumentException("Given type is not an interface.", nameof(constraints));
        }

        if (!forConstraints.TryAdd(interfaceType, constraints.ToArray())) {
            throw new ArgumentException(
                $"Interface `{interfaceType}` is already implemented in `{Name}` type.", nameof(interfaceType)
            );
        }
    }

    internal void ReplaceMethodIdentifier(NeslMethodIdentifier lastIdentifier, NeslMethodBuilder method) {
        if (!methods.TryRemove(lastIdentifier, out _)) {
            throw new ArgumentException(
                $"{nameof(NeslMethod)} with given identifier does not exists in `{Name}` type.",
                nameof(lastIdentifier)
            );
        }

        AddMethodToCollection(method);
    }

    internal void RemoveMethod(NeslMethodBuilder method) {
        methods.TryRemove(new KeyValuePair<NeslMethodIdentifier, NeslMethodBuilder>(method.Identifier, method));
    }

    private bool TryAddMethodToCollection(NeslMethodBuilder method) {
        return methods.TryAdd(method.Identifier, method);
    }

    private void AddMethodToCollection(NeslMethodBuilder method) {
        if (TryAddMethodToCollection(method))
            return;

        throw new InvalidOperationException(
            $"{nameof(NeslMethod)} named `{method.Name}` with given parameter types already exists in `{Name}` type."
        );
    }

}
