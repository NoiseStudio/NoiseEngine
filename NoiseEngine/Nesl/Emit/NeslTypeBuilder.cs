using NoiseEngine.Nesl.Emit.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace NoiseEngine.Nesl.Emit;

public class NeslTypeBuilder : NeslType {

    private readonly Dictionary<uint, NeslField> idToField = new Dictionary<uint, NeslField>();
    private readonly Dictionary<NeslField, uint> fieldToId = new Dictionary<NeslField, uint>();

    private readonly ConcurrentBag<NeslAttribute> attributes = new ConcurrentBag<NeslAttribute>();
    private readonly List<NeslGenericTypeParameterBuilder> genericTypeParameters =
        new List<NeslGenericTypeParameterBuilder>();
    private readonly List<NeslFieldBuilder> fields =
        new List<NeslFieldBuilder>();
    private readonly ConcurrentDictionary<NeslMethodIdentifier, NeslMethodBuilder> methods =
        new ConcurrentDictionary<NeslMethodIdentifier, NeslMethodBuilder>();

    public override IEnumerable<NeslAttribute> Attributes => attributes;
    public override IEnumerable<NeslGenericTypeParameter> GenericTypeParameters => genericTypeParameters;
    public override IReadOnlyList<NeslField> Fields => fields;
    public override IEnumerable<NeslMethod> Methods => methods.Values;

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

            NeslGenericTypeParameterBuilder genericTypeParameter = new NeslGenericTypeParameterBuilder(this, name);
            genericTypeParameters.Add(genericTypeParameter);

            NeslFieldBuilder phantom = DefineField(NeslOperators.Phantom + name, genericTypeParameter);
            phantom.AddAttribute(StaticAttribute.Create());

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

            int index = fields.FindIndex(x => x.Name.StartsWith(NeslOperators.Phantom));
            if (index == -1)
                fields.Add(field);
            else
                fields.Insert(index, field);
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

    internal void ReplaceMethodIdentifier(NeslMethodIdentifier lastIdentifier, NeslMethodBuilder method) {
        if (!methods.TryRemove(lastIdentifier, out _)) {
            throw new ArgumentException(
                $"{nameof(NeslMethod)} with given identifier does not exists in `{Name}` type.",
                nameof(lastIdentifier)
            );
        }

        AddMethodToCollection(method);
    }

    internal uint GetLocalFieldId(NeslField field) {
        lock (idToField) {
            if (!fieldToId.TryGetValue(field, out uint id)) {
                id = (uint)idToField.Count;
                idToField.Add(id, field);
                fieldToId.Add(field, id);
            }

            return id;
        }
    }

    internal override NeslField GetField(uint localFieldId) {
        return idToField[localFieldId];
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
