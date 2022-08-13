using NoiseEngine.Nesl.Emit.Attributes;
using NoiseEngine.Nesl.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NoiseEngine.Nesl;

public abstract class NeslType : INeslGenericTypeParameterOwner {

    private const char Delimiter = '.';

    public abstract IEnumerable<NeslAttribute> Attributes { get; }
    public abstract IEnumerable<NeslGenericTypeParameter> GenericTypeParameters { get; }
    public abstract IEnumerable<NeslField> Fields { get; }
    public abstract IEnumerable<NeslMethod> Methods { get; }

    public virtual string Name => FullName.Substring(FullName.LastIndexOf(Delimiter) + 1);
    public virtual string Namespace => FullName.Substring(0, FullName.LastIndexOf(Delimiter));

    public NeslAssembly Assembly { get; }
    public string FullName { get; }

    public bool IsGeneric => GenericTypeParameters.Any();
    public bool IsClass => !IsValueType;
    public bool IsValueType => Attributes.HasAnyAttribute(nameof(ValueTypeAttribute));

    protected NeslType(NeslAssembly assembly, string fullName) {
        Assembly = assembly;
        FullName = fullName;
    }

    /// <summary>
    /// Finds <see cref="NeslField"/> with given <paramref name="name"/> in this <see cref="NeslType"/>.
    /// </summary>
    /// <param name="name">Name of the searched <see cref="NeslField"/>.</param>
    /// <returns><see cref="NeslField"/> when type was found, <see langword="null"/> when not.</returns>
    public NeslField? GetField(string name) {
        return Fields.FirstOrDefault(x => x.Name == name);
    }

    /// <summary>
    /// Creates final <see cref="NeslType"/> with given <paramref name="typeArguments"/>
    /// from this generic <see cref="NeslType"/>.
    /// </summary>
    /// <param name="typeArguments"><see cref="NeslType"/>s which replaces generic type parameters.</param>
    /// <returns>Final <see cref="NeslType"/> with given <paramref name="typeArguments"/>.</returns>
    /// <exception cref="InvalidOperationException">This <see cref="NeslType"/> is not generic.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The number of given <paramref name="typeArguments"/> does not match
    /// the defined number of generic type parameters.
    /// </exception>
    public NeslType MakeGeneric(params NeslType[] typeArguments) {
        if (!IsGeneric)
            throw new InvalidOperationException($"Type {Name} is not generic.");

        if (GenericTypeParameters.Count() != typeArguments.Length) {
            throw new ArgumentOutOfRangeException(
                nameof(typeArguments),
                $"The number of given {nameof(typeArguments)} does not match the " +
                "defined number of generic type parameters.");
        }

        Dictionary<NeslGenericTypeParameter, NeslType> dictionary =
            new Dictionary<NeslGenericTypeParameter, NeslType>();

        int i = 0;
        foreach (NeslGenericTypeParameter genericTypeParameter in GenericTypeParameters) {
            genericTypeParameter.AssertConstraints(typeArguments[i]);
            dictionary.Add(genericTypeParameter, typeArguments[i]);

            i++;
        }

        SerializedNeslType type = new SerializedNeslType(Assembly, FullName, Attributes.ToImmutableArray());

        Dictionary<uint, NeslField> idToField = new Dictionary<uint, NeslField>();
        Dictionary<NeslField, uint> fieldToId = new Dictionary<NeslField, uint>();

        List<NeslField> fields = new List<NeslField>();
        foreach (NeslField field in Fields) {
            NeslField newField;

            if (field.FieldType is NeslGenericTypeParameter genericTypeParameter) {
                newField = new SerializedNeslField(type, field.Name, dictionary[genericTypeParameter],
                    field.Attributes.ToImmutableArray());
            } else {
                newField = new SerializedNeslField(type, field.Name, field.FieldType,
                    field.Attributes.ToImmutableArray());
            }

            fields.Add(newField);

            uint id = (uint)idToField.Count;
            idToField.Add(id, field);
            fieldToId.Add(field, id);
        }

        type.SetFields(fields.ToImmutableArray(), idToField.ToImmutableDictionary());

        return type;
    }

    internal abstract NeslField GetField(uint localFieldId);

}
