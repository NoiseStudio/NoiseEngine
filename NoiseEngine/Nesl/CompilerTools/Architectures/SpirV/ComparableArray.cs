using System;
using System.Linq;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal class ComparableArray : IEquatable<ComparableArray> {

    private readonly object[] values;

    public ComparableArray(object[] values) {
        this.values = values;
    }

    public bool Equals(ComparableArray? other) {
        if (other is null)
            return false;
        return other.values.SequenceEqual(values);
    }

    public override int GetHashCode() {
        int result = 17;

        foreach (object obj in values)
            result = result * 23 + obj.GetHashCode();

        return result;
    }

}
