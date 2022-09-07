using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace NoiseEngine.Collections;

public class ReadOnlyListEqualityComparer<T> : IEqualityComparer<IReadOnlyList<T>> {

    /// <summary>
    /// Determines whether the specified objects are equal.
    /// </summary>
    /// <param name="x">The first object of type T to compare.</param>
    /// <param name="y">The second object of type T to compare.</param>
    /// <returns>
    /// <see langword="true"/> if the specified objects are equal; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(IReadOnlyList<T>? x, IReadOnlyList<T>? y) {
        if (x is null || y is null || x.Count != y.Count)
            return false;
        return x.SequenceEqual(y);
    }

    /// <summary>
    /// Returns a hash code for the specified object.
    /// </summary>
    /// <param name="obj">The <see cref="IReadOnlyList{T}"/> for which a hash code is to be returned.</param>
    /// <returns>A hash code for the specified object.</returns>
    public int GetHashCode([DisallowNull] IReadOnlyList<T> obj) {
        int result = 17;

        foreach (T element in obj) {
            if (element is not null)
                result = result * 23 + element.GetHashCode();
            else
                result *= 31;
        }

        return result;
    }

}
