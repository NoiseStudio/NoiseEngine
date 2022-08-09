using NoiseEngine.Collections;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.Emit;

public class NeslTypeBuilder : NeslType {

    private readonly Dictionary<uint, NeslField> idToField = new Dictionary<uint, NeslField>();
    private readonly Dictionary<NeslField, uint> fieldToId = new Dictionary<NeslField, uint>();

    private readonly ConcurrentBag<NeslAttribute> attributes = new ConcurrentBag<NeslAttribute>();
    private readonly ConcurrentDictionary<string, NeslFieldBuilder> fields =
        new ConcurrentDictionary<string, NeslFieldBuilder>();
    private readonly ConcurrentDictionary<NeslMethodIdentifier, NeslMethodBuilder> methods =
        new ConcurrentDictionary<NeslMethodIdentifier, NeslMethodBuilder>();

    public override IEnumerable<NeslAttribute> Attributes => attributes;
    public override IEnumerable<NeslField> Fields => fields.Values;
    public override IEnumerable<NeslMethod> Methods => methods.Values;

    internal NeslTypeBuilder(NeslAssemblyBuilder assembly, string fullName) : base(assembly, fullName) {
    }

    /// <summary>
    /// Creates new <see cref="NeslFieldBuilder"/> in this type.
    /// </summary>
    /// <param name="name">Name of new <see cref="NeslFieldBuilder"/>.</param>
    /// <param name="fieldType"><see cref="NeslType"/> of new field.</param>
    /// <returns>New <see cref="NeslFieldBuilder"/>.</returns>
    /// <exception cref="ArgumentException">
    /// <see cref="NeslMethod"/> with this <paramref name="name"/> already exists in this type.
    /// </exception>
    public NeslFieldBuilder DefineField(string name, NeslType fieldType) {
        NeslFieldBuilder field = new NeslFieldBuilder(this, name, fieldType);

        if (!fields.TryAdd(name, field)) {
            throw new ArgumentException($"{nameof(NeslMethod)} named `{name}` already exists in `{Name}` type.",
                nameof(name));
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
    /// <exception cref="ArgumentException">
    /// <see cref="NeslMethod"/> with this <paramref name="name"/> already exists in this type.
    /// </exception>
    public NeslMethodBuilder DefineMethod(string name, NeslType? returnType = null, params NeslType[] parameterTypes) {
        NeslMethodBuilder method = new NeslMethodBuilder(this, name, returnType, parameterTypes);

        if (!methods.TryAdd(new NeslMethodIdentifier(name, new ComparableArray<NeslType>(parameterTypes)), method)) {
            throw new ArgumentException($"{nameof(NeslMethod)} named `{name}` already exists in `{Name}` type.",
                nameof(name));
        }

        return method;
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

}
