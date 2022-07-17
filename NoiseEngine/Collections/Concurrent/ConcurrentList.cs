using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NoiseEngine.Collections.Concurrent {
    public class ConcurrentList<T> : ICollection<T>, IReadOnlyCollection<T>, ICollection {

        private const int DefaultCapacity = 4;

        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

        private T[] items;
        private int count;
        private int ensureCount;
        private int newCount;

        public int Count => count;
        private int Capacity => items.Length;

        object ICollection.SyncRoot => items.SyncRoot;
        bool ICollection.IsSynchronized => true;
        bool ICollection<T>.IsReadOnly => false;

        public ConcurrentList(int capacity = DefaultCapacity) {
            items = new T[capacity];
        }

        public ConcurrentList(ICollection<T> items) {
            this.items = new T[items.Count];
            items.CopyTo(this.items, 0);
            count = items.Count;
        }

        public ConcurrentList(ReadOnlySpan<T> items) {
            this.items = new T[items.Length];
            items.CopyTo(AsSpan(0, items.Length));
            count = items.Length;
        }

        public ConcurrentList(IEnumerable<T> items) {
            this.items = new T[items.Count()];
            count = this.items.Length;

            int i = 0;
            foreach (T item in items)
                this.items[i++] = item;
        }

        public ConcurrentList(T[] items) : this((ICollection<T>)items) {
        }

        ~ConcurrentList() {
            locker.Dispose();
        }

        /// <summary>
        /// Adds <paramref name="item"/> to the <see cref="ConcurrentList{T}"/>.
        /// </summary>
        /// <param name="item">Item to add.</param>
        public void Add(T item) {
            EnsureCapacity(Interlocked.Increment(ref ensureCount));

            locker.EnterReadLock();
            items[Interlocked.Increment(ref newCount) - 1] = item;
            locker.ExitReadLock();
            Interlocked.Increment(ref count);
        }

        /// <summary>
        /// Adds <paramref name="items"/> to the <see cref="ConcurrentList{T}"/>.
        /// </summary>
        /// <param name="items">Items to add.</param>
        public void AddRange(ICollection<T> items) {
            EnsureCapacity(Interlocked.Add(ref ensureCount, items.Count));

            locker.EnterReadLock();
            items.CopyTo(this.items, Interlocked.Add(ref newCount, items.Count) - items.Count);
            locker.ExitReadLock();
            Interlocked.Add(ref count, items.Count);
        }

        /// <summary>
        /// Adds <paramref name="items"/> to the <see cref="ConcurrentList{T}"/>.
        /// </summary>
        /// <param name="items">Items to add.</param>
        public void AddRange(ReadOnlySpan<T> items) {
            EnsureCapacity(Interlocked.Add(ref ensureCount, items.Length));

            locker.EnterReadLock();
            items.CopyTo(AsSpan(Interlocked.Add(ref newCount, items.Length) - items.Length, items.Length));
            locker.ExitReadLock();
            Interlocked.Add(ref count, items.Length);
        }

        /// <summary>
        /// Adds <paramref name="items"/> to the <see cref="ConcurrentList{T}"/>.
        /// </summary>
        /// <param name="items">Items to add.</param>
        public void AddRange(IEnumerable<T> items) {
            AddRange(items.ToArray());
        }

        /// <summary>
        /// Adds <paramref name="items"/> to the <see cref="ConcurrentList{T}"/>.
        /// </summary>
        /// <param name="items">Items to add.</param>
        public void AddRange(T[] items) {
            AddRange((ICollection<T>)items);
        }

        /// <summary>
        /// Removes all elements from the <see cref="ConcurrentList{T}"/>.
        /// </summary>
        public void Clear() {
            if (count == 0)
                return;

            locker.EnterWriteLock();

            Array.Clear(items, 0, count);
            count = 0;
            newCount = 0;
            ensureCount = 0;

            locker.ExitWriteLock();
        }

        /// <summary>
        /// Determines whether the <see cref="ConcurrentList{T}"/> contains specific value.
        /// </summary>
        /// <param name="item">The item to locate in the <see cref="ConcurrentList{T}"/>.</param>
        /// <returns><see langword="true"/> if <paramref name="item"/> is found in the <see cref="ConcurrentList{T}"/>;
        /// otherwise, <see langword="false"/>.</returns>
        public bool Contains(T item) {
            foreach (T i in this) {
                if (i!.Equals(item))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Copies the elements of the <see cref="ConcurrentList{T}"/> to an <see cref="Array"/>,
        /// starting at a particular <see cref="Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the elements copied
        /// from <see cref="ConcurrentList{T}"/>. The <see cref="Array"/> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        public void CopyTo(T[] array, int arrayIndex) {
            AsSpan().CopyTo(array.AsSpan(arrayIndex));
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="ConcurrentList{T}"/>.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the <see cref="ConcurrentList{T}"/>.</returns>
        public IEnumerator<T> GetEnumerator() {
            locker.EnterReadLock();
            T[] items = this.items;
            int count = Count;
            locker.ExitReadLock();

            for (int i = 0; i < count; i++)
                yield return items[i];
        }

        /// <summary>
        /// Removes the first occurrence of a specific <paramref name="item"/> from the <see cref="ConcurrentList{T}"/>.
        /// </summary>
        /// <param name="item">The item to remove from the <see cref="ConcurrentList{T}"/>.</param>
        /// <returns><see langword="true"/> if item was successfully removed from the <see cref="ConcurrentList{T}"/>;
        /// otherwise, <see langword="false"/>. This method also returns <see langword="false"/> if item is not found in
        /// the original <see cref="ConcurrentList{T}"/>.</returns>
        public bool Remove(T item) {
            for (int i = 0; i < count; i++) {
                if (!items[i]!.Equals(item))
                    continue;

                locker.EnterWriteLock();

                if (!items[i]!.Equals(item)) {
                    for (i = 0; i < count; i++) {
                        if (items[i]!.Equals(item))
                            break;
                    }

                    if (i == count) {
                        locker.ExitWriteLock();
                        return false;
                    }
                }

                int indexP = i + 1;
                AsSpan(indexP, count - indexP).CopyTo(AsSpan(i, count - i));
                items[--count] = default!;
                ensureCount--;
                newCount--;

                locker.ExitWriteLock();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Copies the elements of the <see cref="ConcurrentList{T}"/> to a new array.
        /// </summary>
        /// <returns>An array containing copies of the elements of the <see cref="ConcurrentList{T}"/>.</returns>
        public T[] ToArray() {
            T[] result = new T[Count];
            Array.Copy(items, result, result.Length);
            return result;
        }

        /// <summary>
        /// Creates a new span over this <see cref="ConcurrentList{T}"/>.
        /// </summary>
        /// <returns>The span representation of the <see cref="ConcurrentList{T}"/>.</returns>
        public Span<T> AsSpan() {
            return items.AsSpan(0, count);
        }

        /// <summary>
        /// Creates a new span over a portion of this <see cref="ConcurrentList{T}"/> starting at a specified
        /// position to the end of the <see cref="ConcurrentList{T}"/>.
        /// </summary>
        /// <param name="start">The initial index from which the
        /// <see cref="ConcurrentList{T}"/> will be converted.</param>
        /// <returns>The span representation of the <see cref="ConcurrentList{T}"/>.</returns>
        public Span<T> AsSpan(int start) {
            return items.AsSpan(start, count - start);
        }

        /// <summary>
        /// Creates a new span over a portion of this <see cref="ConcurrentList{T}"/> starting at a specified
        /// position for a specified length.
        /// </summary>
        /// <param name="start">The initial index from which the
        /// <see cref="ConcurrentList{T}"/> will be converted.</param>
        /// <param name="length">The number of items in the span.</param>
        /// <returns>The span representation of the <see cref="ConcurrentList{T}"/>.</returns>
        public Span<T> AsSpan(int start, int length) {
            return items.AsSpan(start, length);
        }

        private void EnsureCapacity(int minCapacity) {
            if (minCapacity <= Capacity)
                return;

            locker.EnterWriteLock();

            int newCapacity = Capacity == 0 ? DefaultCapacity : Capacity * 2;
            if (newCapacity < minCapacity)
                newCapacity = minCapacity;

            Array.Resize(ref items, newCapacity);

            locker.ExitWriteLock();
        }

        void ICollection.CopyTo(Array array, int index) {
            items.CopyTo(array, index);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

    }
}
