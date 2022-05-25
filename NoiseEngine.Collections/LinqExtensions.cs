using System.Collections.Generic;

namespace NoiseEngine.Collections {
    public static class LinqExtensions {

        public static FastList<T> ToFastList<T>(this IEnumerable<T> enumerable) {
            return new FastList<T>(enumerable);
        }

    }
}
