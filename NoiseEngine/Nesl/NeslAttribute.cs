using NoiseEngine.Nesl.Serialization;
using NoiseEngine.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace NoiseEngine.Nesl;

public abstract class NeslAttribute : ISerializable<NeslAttribute> {

    public string FullName { get; init; }
    public AttributeTargets Targets { get; init; }
    public ImmutableArray<byte> Bytes { get; init; }

    public NeslAttribute() {
        FullName = string.Empty;
    }

    /// <summary>
    /// Creates new <see cref="NeslAttribute"/>.
    /// </summary>
    /// <param name="fullName">Full name.</param>
    /// <param name="targets">Attribute targets.</param>
    /// <param name="bytes">Bytes of attribute tail.</param>
    /// <returns><see cref="NeslAttribute"/> with given parameters.</returns>
    public static NeslAttribute Create(string fullName, AttributeTargets targets, IEnumerable<byte>? bytes) {
        return new SerializedNeslAttribute {
            FullName = fullName,
            Targets = targets,
            Bytes = bytes?.ToImmutableArray() ?? ImmutableArray<byte>.Empty
        };
    }

    /// <summary>
    /// Creates new <see cref="NeslAttribute"/> with data from <paramref name="reader"/>.
    /// </summary>
    /// <param name="reader"><see cref="SerializationReader"/>.</param>
    /// <returns>New <see cref="NeslAttribute"/> with data from <paramref name="reader"/>.</returns>
    public static NeslAttribute Deserialize(SerializationReader reader) {
        return Create(
            reader.ReadString(), (AttributeTargets)reader.ReadUInt32(),
            reader.ReadBool() ? reader.ReadEnumerableUInt8() : null
        );
    }

    /// <summary>
    /// Checks if that properties have valid values.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when attribute properties are valid; otherwise <see langword="false"/>.
    /// </returns>
    public abstract bool CheckIsValid();

    /// <summary>
    /// Asserts that properties have valid values.
    /// </summary>
    /// <exception cref="InvalidOperationException">Not all properties have valid values.</exception>
    public void AssertValid() {
        if (!CheckIsValid())
            throw new InvalidOperationException("A different attribute properties was expected.");
    }

    /// <summary>
    /// Casts this <see cref="NeslAttribute"/> to <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Result type of attribute.</typeparam>
    /// <returns><typeparamref name="T"/> attribute.</returns>
    public T Cast<T>() where T : NeslAttribute, new() {
        Type type = GetType();
        do {
            if (typeof(T) == type)
                return (T)this;
        } while ((type = type.BaseType!) != typeof(object));

        T obj = new T {
            FullName = FullName,
            Targets = Targets,
            Bytes = Bytes
        };

        obj.AssertValid();
        return obj;
    }

    /// <summary>
    /// Tries casts this <see cref="NeslAttribute"/> to <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Result type of attribute.</typeparam>
    /// <param name="attribute">Casted attribute.</param>
    /// <returns><see langword="true"/> if the cast was successful; otherwise <see langword="false"/>.</returns>
    public bool TryCast<T>([NotNullWhen(true)] out T? attribute) where T : NeslAttribute, new() {
        Type type = GetType();
        do {
            if (typeof(T) == type) {
                attribute = (T)this;
                return true;
            }
        } while ((type = type.BaseType!) != typeof(object));

        T obj = new T {
            FullName = FullName,
            Targets = Targets,
            Bytes = Bytes
        };

        if (obj.CheckIsValid()) {
            attribute = obj;
            return true;
        }

        attribute = null;
        return false;
    }

    internal NeslAttribute RemoveGenericsInternal(
        IReadOnlyDictionary<NeslGenericTypeParameter, NeslType> targetTypes
    ) {
        return RemoveGenerics(targetTypes);
    }

    /// <summary>
    /// Removes generics from this <see cref="NeslAttribute"/> with given <paramref name="targetTypes"/>.
    /// </summary>
    /// <param name="targetTypes">
    /// <see cref="IReadOnlyDictionary{TKey, TValue}"/> where TKey is original generic type and TValue is target type.
    /// </param>
    /// <returns>Returns new or this <see cref="NeslAttribute"/> without generics.</returns>
    protected virtual NeslAttribute RemoveGenerics(
        IReadOnlyDictionary<NeslGenericTypeParameter, NeslType> targetTypes
    ) {
        return this;
    }

    /// <summary>
    /// Checks if that <see cref="FullName"/> property have valid value.
    /// </summary>
    /// <param name="expectedFullName">Expected full name value.</param>
    /// <returns>
    /// <see langword="true"/> when attribute full name is valid; otherwise <see langword="false"/>.
    /// </returns>
    protected bool CheckIfValidFullName(string expectedFullName) {
        return expectedFullName == FullName;
    }

    /// <summary>
    /// Checks if that <see cref="Targets"/> property have valid value.
    /// </summary>
    /// <param name="expectedTargets">Expected targets value.</param>
    /// /// <returns>
    /// <see langword="true"/> when attribute targets is valid; otherwise <see langword="false"/>.
    /// </returns>
    protected bool CheckIfValidTargets(AttributeTargets expectedTargets) {
        return expectedTargets == Targets;
    }

    /// <summary>
    /// Checks if that <see cref="Bytes"/> property have valid value.
    /// </summary>
    /// <param name="expectedLength">Expected length of <see cref="Bytes"/> property.</param>
    /// /// <returns>
    /// <see langword="true"/> when attribute has valid <see cref="Bytes"/> length; otherwise <see langword="false"/>.
    /// </returns>
    protected bool CheckIfValidBytesLength(int expectedLength) {
        return (expectedLength == -1 && Bytes.IsDefault) || (Bytes.Length == expectedLength);
    }

    /// <summary>
    /// Serializes this <see cref="NeslAttribute"/> and writes it to the <paramref name="writer"/>.
    /// </summary>
    /// <param name="writer"><see cref="SerializationWriter"/>.</param>
    public void Serialize(SerializationWriter writer) {
        writer.WriteString(FullName);
        writer.WriteUInt32((uint)Targets);

        if (Bytes.IsDefault) {
            writer.WriteBool(false);
        } else {
            writer.WriteBool(true);
            writer.WriteEnumerable(Bytes);
        }
    }

}
