using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace NoiseEngine.Nesl;

public abstract class NeslAttribute {

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
    public static NeslAttribute Create(string fullName, AttributeTargets targets, byte[] bytes) {
        return new SerializedNeslAttribute {
            FullName = fullName,
            Targets = targets,
            Bytes = bytes.ToImmutableArray()
        };
    }

    /// <summary>
    /// Asserts that properties have valid values.
    /// </summary>
    /// <exception cref="InvalidOperationException">Not all properties have valid values.</exception>
    public abstract void AssertValid();

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

        try {
            obj.AssertValid();
        } catch (InvalidOperationException) {
            attribute = null;
            return false;
        }

        attribute = obj;
        return true;
    }

    /// <summary>
    /// Asserts that <see cref="FullName"/> property have valid value.
    /// </summary>
    /// <param name="expectedFullName">Expected full name value.</param>
    /// <exception cref="InvalidOperationException">Invalid <see cref="FullName"/> property value.</exception>
    protected void AssertValidFullName(string expectedFullName) {
        if (expectedFullName != FullName)
            throw new InvalidOperationException("A different attribute full name was expected.");
    }

    /// <summary>
    /// Asserts that <see cref="Targets"/> property have valid value.
    /// </summary>
    /// <param name="expectedTargets">Expected targets value.</param>
    /// <exception cref="InvalidOperationException">Invalid <see cref="Targets"/> property value.</exception>
    protected void AssertValidTargets(AttributeTargets expectedTargets) {
        if (expectedTargets != Targets)
            throw new InvalidOperationException("A different attribute targets was expected.");
    }

    /// <summary>
    /// Asserts that <see cref="Bytes"/> property have valid value.
    /// </summary>
    /// <param name="expectedLength">Expected length of <see cref="Bytes"/> property.</param>
    /// <exception cref="InvalidOperationException">Invalid <see cref="Bytes"/> property value.</exception>
    protected void AssertValidBytesLength(int expectedLength) {
        if (expectedLength == -1 && Bytes.IsDefault)
            return;

        if (Bytes.Length == expectedLength)
            return;

        throw new InvalidOperationException("A different attribute bytes length was expected.");
    }

}
