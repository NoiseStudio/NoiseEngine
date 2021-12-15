using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NoiseStudio.JobsAg {
    internal class ConcurrentHashSet<T> : IEnumerable<T>, ISet<T> where T : notnull {

        private readonly ConcurrentDictionary<T, byte> dictionary = new ConcurrentDictionary<T, byte>();

        public int Count => dictionary.Count;
        public bool IsReadOnly => ((ICollection<T>)dictionary).IsReadOnly;

        public bool Add(T item) {
            return dictionary.TryAdd(item, 0);
        }

        public void Clear() {
            dictionary.Clear();
        }

        public bool Contains(T item) {
            return dictionary.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            foreach (KeyValuePair<T, byte> keyValuePair in dictionary)
                array[arrayIndex++] = keyValuePair.Key;
        }

        public void ExceptWith(IEnumerable<T> other) {
            if (other == null) {
                throw new ArgumentNullException(nameof(other));
            }

            if (Count == 0) {
                return;
            }

            if (other == this) {
                Clear();
                return;
            }

            foreach (T element in other) {
                Remove(element);
            }
        }

        public IEnumerator<T> GetEnumerator() {
            foreach (KeyValuePair<T, byte> keyValuePair in dictionary)
                yield return keyValuePair.Key;
        }

        public void IntersectWith(IEnumerable<T> other) {
            throw new NotImplementedException();
        }

        public bool IsProperSubsetOf(IEnumerable<T> other) {
            throw new NotImplementedException();
        }

        public bool IsProperSupersetOf(IEnumerable<T> other) {
            throw new NotImplementedException();
        }

        public bool IsSubsetOf(IEnumerable<T> other) {
            throw new NotImplementedException();
        }

        public bool IsSupersetOf(IEnumerable<T> other) {
            throw new NotImplementedException();
        }

        public bool Overlaps(IEnumerable<T> other) {
            if (other == null) {
                throw new ArgumentNullException(nameof(other));
            }

            if (Count == 0) {
                return false;
            }

            foreach (T element in other) {
                if (Contains(element))
                    return true;
            }
            return false;
        }

        public bool Remove(T item) {
            return dictionary.TryRemove(item, out _);
        }

        public bool SetEquals(IEnumerable<T> other) {
            throw new NotImplementedException();
        }

        public void SymmetricExceptWith(IEnumerable<T> other) {
            throw new NotImplementedException();
        }

        public void UnionWith(IEnumerable<T> other) {
            if (other == null) {
                throw new ArgumentNullException(nameof(other));
            }

            foreach (T item in other) {
                Add(item);
            }
        }

        void ICollection<T>.Add(T item) {
            Add(item);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

    }
}
