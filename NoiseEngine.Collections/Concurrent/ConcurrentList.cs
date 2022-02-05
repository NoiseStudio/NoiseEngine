using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace NoiseEngine.Collections.Concurrent {
    public class ConcurrentList<T> :
        IEnumerable<T>, IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, ICollection<T>, IList, ICollection
    {

        private readonly List<T> list = new List<T>();
        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public T this[int index] {
            get {
                locker.EnterReadLock();
                T item;
                try {
                    item = list[index];
                } finally {
                    locker.ExitReadLock();
                }
                return item;
            }
            set {
                locker.EnterWriteLock();
                try {
                    list[index] = value;
                } finally {
                    locker.ExitWriteLock();
                }
            }
        }

        public int Count => list.Count;

        object? IList.this[int index] {
            get {
                return this[index];
            }
            set {
                this[index] = (T)value!;
            }
        }

        bool ICollection<T>.IsReadOnly => ((ICollection<T>)list).IsReadOnly;
        bool ICollection.IsSynchronized => ((ICollection)list).IsSynchronized;
        object ICollection.SyncRoot => ((ICollection)list).SyncRoot;
        bool IList.IsFixedSize => ((IList)list).IsFixedSize;
        bool IList.IsReadOnly => ((IList)list).IsReadOnly;

        private static bool IsCompatibleObject(object? value) {
            return (value is T) || (value == null && default(T) == null);
        }

        /// <summary>
        /// Adds <paramref name="item"/> to the <see cref="ConcurrentList{T}"/>.
        /// </summary>
        /// <param name="item">Being added item.</param>
        public void Add(T item) {
            locker.EnterWriteLock();
            list.Add(item);
            locker.ExitWriteLock();
        }

        /// <summary>
        /// Removes all elements from the <see cref="ConcurrentList{T}"/>.
        /// </summary>
        public void Clear() {
            locker.EnterWriteLock();
            list.Clear();
            locker.ExitWriteLock();
        }

        /// <summary>
        /// Determines whether the <see cref="ConcurrentList{T}"/> contains specific value.
        /// </summary>
        /// <param name="item">The item to locate in the <see cref="ConcurrentList{T}"/>.</param>
        /// <returns><see langword="true"/> if <paramref name="item"/> is found in the <see cref="ConcurrentList{T}"/>;
        /// otherwise, <see langword="false"/>.</returns>
        public bool Contains(T item) {
            locker.EnterReadLock();
            bool contains = list.Contains(item);
            locker.ExitReadLock();
            return contains;
        }

        /// <summary>
        /// Copies the elements of the <see cref="ConcurrentList{T}"/> to an <see cref="Array"/>,
        /// starting at a particular <see cref="Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the elements copied
        /// from <see cref="ConcurrentList{T}"/>. The <see cref="Array"/> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        public void CopyTo(T[] array, int arrayIndex) {
            locker.EnterReadLock();
            try {
                list.CopyTo(array, arrayIndex);
            } finally {
                locker.ExitReadLock();
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="ConcurrentList{T}"/>.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the <see cref="ConcurrentList{T}"/>.</returns>
        public IEnumerator<T> GetEnumerator() {
            locker.EnterReadLock();
            for (int i = 0; i < Count; i++)
                yield return list[i];
            locker.ExitReadLock();
        }

        /// <summary>
        /// Determines the index of a specific <paramref name="item"/> in the <see cref="ConcurrentList{T}"/>.
        /// </summary>
        /// <param name="item">The item to locate in the <see cref="ConcurrentList{T}"/>.</param>
        /// <returns>The index of item if found in the <see cref="ConcurrentList{T}"/>; otherwise, -1.</returns>
        public int IndexOf(T item) {
            locker.EnterReadLock();
            int index = list.IndexOf(item);
            locker.ExitReadLock();
            return index;
        }

        /// <summary>
        /// Inserts an item to the <see cref="ConcurrentList{T}"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
        /// <param name="item">The item to insert into the <see cref="ConcurrentList{T}"/>.</param>
        public void Insert(int index, T item) {
            locker.EnterWriteLock();
            try {
                list.Insert(index, item);
            } finally {
                locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific <paramref name="item"/> from the <see cref="ConcurrentList{T}"/>.
        /// </summary>
        /// <param name="item">The item to remove from the <see cref="ConcurrentList{T}"/>.</param>
        /// <returns><see langword="true"/> if item was successfully removed from the <see cref="ConcurrentList{T}"/>;
        /// otherwise, <see langword="false"/>. This method also returns <see langword="false"/> if item is not found in
        /// the original <see cref="ConcurrentList{T}"/>.</returns>
        public bool Remove(T item) {
            locker.EnterWriteLock();
            bool removed = list.Remove(item);
            locker.ExitWriteLock();
            return removed;
        }

        /// <summary>
        /// Removes the <see cref="ConcurrentList{T}"/> item at the specified <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        public void RemoveAt(int index) {
            locker.EnterWriteLock();
            try {
                list.RemoveAt(index);
            } finally {
                locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Performs <paramref name="action"/> with exclusive write access to this <see cref="ConcurrentList{T}"/>.
        /// </summary>
        /// <param name="action">Performed <see cref="Action"/>.</param>
        public void WriteWork(Action action) {
            locker.EnterWriteLock();
            try {
                action.Invoke();
            } finally {
                locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Copies the elements of the <see cref="ConcurrentList{T}"/> to a new array.
        /// </summary>
        /// <returns>An array containing copies of the elements of the <see cref="ConcurrentList{T}"/>.</returns>
        public T[] ToArray() {
            locker.EnterReadLock();
            T[] result = list.ToArray();
            locker.ExitReadLock();
            return result;
        }

        void ICollection.CopyTo(Array array, int index) {
            locker.EnterReadLock();
            try {
                ((ICollection)list).CopyTo(array, index);
            } finally {
                locker.ExitReadLock();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        int IList.Add(object? value) {
            if (!IsCompatibleObject(value))
                return -1;

            locker.EnterWriteLock();
            int result = list.Count;
            list.Add((T)value!);
            locker.ExitWriteLock();
            return result;
        }

        bool IList.Contains(object? value) {
            if (IsCompatibleObject(value))
                return Contains((T)value!);
            return false;
        }

        int IList.IndexOf(object? value) {
            if (IsCompatibleObject(value))
                return IndexOf((T)value!);
            return -1;
        }

        void IList.Insert(int index, object? value) {
            if (IsCompatibleObject(value))
                Insert(index, (T)value!);
        }

        void IList.Remove(object? value) {
            if (IsCompatibleObject(value))
                Remove((T)value!);
        }

    }
}
