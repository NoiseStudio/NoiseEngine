using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NoiseEngine.Collections;

public class EquatableReadOnlyList<T> : IReadOnlyList<T>, IEquatable<EquatableReadOnlyList<T>> {

    private readonly IReadOnlyList<T> list;
    private readonly int hashCode;

    public T this[int index] => list[index];
    public int Count => list.Count;

    public EquatableReadOnlyList(IReadOnlyList<T> list) {
        this.list = list;

        hashCode = 17;
        foreach (T obj in list) {
            if (obj is not null)
                hashCode = hashCode * 23 + obj.GetHashCode();
            else
                hashCode *= 31;
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="EquatableReadOnlyList{T}"/>.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the <see cref="EquatableReadOnlyList{T}"/>.</returns>
    public IEnumerator<T> GetEnumerator() {
        return list.GetEnumerator();
    }

    /// <summary>
    /// Returns a value indicating whether this <see cref="EquatableReadOnlyList{T}"/>
    /// is equal to a <paramref name="other"/>.
    /// </summary>
    /// <param name="other">
    /// An <see cref="EquatableReadOnlyList{T}"/> to compare to this <see cref="EquatableReadOnlyList{T}"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="other"/> elements is equals to elements
    /// of this <see cref="EquatableReadOnlyList{T}"/>; otherwise <see langword="false"/>.
    /// </returns>
    public bool Equals(EquatableReadOnlyList<T>? other) {
        if (other is null)
            return false;
        return other.list.SequenceEqual(list);
    }

    /// <summary>
    /// Returns a value indicating whether this instance is equal to a specified object.
    /// </summary>
    /// <param name="obj">An object to compare to this instance.</param>
    /// <returns>
    /// <see langword="true"/> if obj is an instance of <see cref="EquatableReadOnlyList{T}"/>
    /// and equals the value of this instance; otherwise <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj) {
        return obj is EquatableReadOnlyList<T> a && Equals(a);
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() {
        return hashCode;
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

}
