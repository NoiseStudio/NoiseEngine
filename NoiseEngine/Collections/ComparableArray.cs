using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NoiseEngine.Collections;

public class ComparableArray<T> : IReadOnlyList<T>, IEquatable<ComparableArray<T>> {

    private readonly T[] values;

    public T this[int index] {
        get => values[index];
        set => values[index] = value;
    }

    public int Count => values.Length;

    public ComparableArray(T[] values) {
        this.values = values;
    }

    /// <summary>
    /// Returns array decorated by this <see cref="ComparableArray{T}"/>.
    /// </summary>
    /// <returns>An array decorated by this <see cref="ComparableArray{T}"/>.</returns>
    public T[] ToArray() {
        return values;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="ComparableArray{T}"/>.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the <see cref="ComparableArray{T}"/>.</returns>
    public IEnumerator<T> GetEnumerator() {
        foreach (T value in values)
            yield return value;
    }

    /// <summary>
    /// Returns a value indicating whether this <see cref="ComparableArray{T}"/>
    /// is equal to a <paramref name="other"/>.
    /// </summary>
    /// <param name="other">
    /// An <see cref="ComparableArray{T}"/> to compare to this <see cref="ComparableArray{T}"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="other"/> elements is equals to elements
    /// of this <see cref="ComparableArray{T}"/>; otherwise <see langword="false"/>.
    /// </returns>
    public bool Equals(ComparableArray<T>? other) {
        if (other is null)
            return false;
        return other.values.SequenceEqual(values);
    }

    /// <summary>
    /// Returns a value indicating whether this instance is equal to a specified object.
    /// </summary>
    /// <param name="obj">An object to compare to this instance.</param>
    /// <returns>
    /// <see langword="true"/> if obj is an instance of <see cref="ComparableArray{T}"/>
    /// and equals the value of this instance; otherwise <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj) {
        return obj is ComparableArray<T> a && Equals(a);
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() {
        int result = 17;

        foreach (T obj in values) {
            if (obj is not null)
                result = result * 23 + obj.GetHashCode();
            else
                result *= 31;
        }

        return result;
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

}
