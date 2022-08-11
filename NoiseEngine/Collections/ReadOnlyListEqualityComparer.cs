using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace NoiseEngine.Collections;

public class ReadOnlyListEqualityComparer<T> : IEqualityComparer<IReadOnlyList<T>> {

    public bool Equals(IReadOnlyList<T>? x, IReadOnlyList<T>? y) {
        if (x is null || y is null || x.Count != y.Count)
            return false;
        return x.SequenceEqual(y);
    }

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
