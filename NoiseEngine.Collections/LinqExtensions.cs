using System.Collections.Generic;

namespace NoiseEngine.Collections {
    public static class LinqExtensions {

        /// <summary>
        /// Converts <paramref name="enumerable"/> to <see cref="FastList{T}"/>.
        /// </summary>
        /// <typeparam name="T"><see cref="System.Type"/> of elements in <see cref="IEnumerable{T}"/>
        /// and <see cref="FastList{T}"/>.</typeparam>
        /// <param name="enumerable"><see cref="IEnumerable{T}"/> which converts to <see cref="FastList{T}"/>.</param>
        /// <returns>New <see cref="FastList{T}"/> from <see cref="IEnumerable{T}"/>.</returns>
        public static FastList<T> ToFastList<T>(this IEnumerable<T> enumerable) {
            return new FastList<T>(enumerable);
        }

    }
}
