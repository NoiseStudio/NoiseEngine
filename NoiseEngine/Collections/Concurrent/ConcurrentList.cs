using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NoiseEngine.Collections.Concurrent {
    public class ConcurrentList<T> : ICollection<T>, IReadOnlyCollection<T>, ICollection {

        private const int InitialSegmentCapacity = 32;
        private const int MaxSegmentCapacity = 1024 * 1024;

        private ConcurrentListSegment<T> head;
        private object? syncRoot;

        public int Count {
            get {
                int count = 0;

                ConcurrentListSegment<T>? segment = head;
                do {
                    count += segment.Count;
                    segment = segment.Previous;
                } while (segment is not null);

                return count;
            }
        }

        object ICollection.SyncRoot {
            get {
                Interlocked.CompareExchange(ref syncRoot, new object(), null);
                return syncRoot;
            }
        }

        bool ICollection.IsSynchronized => true;
        bool ICollection<T>.IsReadOnly => false;

        public ConcurrentList(int capacity = InitialSegmentCapacity) {
            head = new ConcurrentListSegment<T>(null, capacity);
        }

        public ConcurrentList(IEnumerable<T> items) {
            head = new ConcurrentListSegment<T>(null,
                items.Select(x => new ConcurrentListSegmentValue<T>(x)).ToArray());
        }

        public ConcurrentList(ReadOnlySpan<T> items) {
            ConcurrentListSegmentValue<T>[] result = new ConcurrentListSegmentValue<T>[items.Length];
            for (int i = 0; i < items.Length; i++)
                result[i] = new ConcurrentListSegmentValue<T>(items[i]);

            head = new ConcurrentListSegment<T>(null, result);
        }

        /// <summary>
        /// Adds <paramref name="item"/> to the <see cref="ConcurrentList{T}"/>.
        /// </summary>
        /// <param name="item">Item to add.</param>
        public void Add(T item) {
            ConcurrentListSegment<T>? segment = head;
            while (!segment.TryAdd(item)) {
                CreateNextSegmentCompare(segment);
                segment = head;
            }
        }

        /// <summary>
        /// Adds <paramref name="items"/> to the <see cref="ConcurrentList{T}"/>.
        /// </summary>
        /// <param name="items">Items to add.</param>
        public void AddRange(IEnumerable<T> items) {
            foreach (T item in items)
                Add(item);
        }

        /// <summary>
        /// Adds <paramref name="items"/> to the <see cref="ConcurrentList{T}"/>.
        /// </summary>
        /// <param name="items">Items to add.</param>
        public void AddRange(ReadOnlySpan<T> items) {
            foreach (T item in items)
                Add(item);
        }

        /// <summary>
        /// Removes all elements from the <see cref="ConcurrentList{T}"/>.
        /// </summary>
        public void Clear() {
            lock (head)
                head = new ConcurrentListSegment<T>(null, Math.Min(Count, MaxSegmentCapacity));
        }

        /// <summary>
        /// Determines whether the <see cref="ConcurrentList{T}"/> contains specific value.
        /// </summary>
        /// <param name="item">The item to locate in the <see cref="ConcurrentList{T}"/>.</param>
        /// <returns><see langword="true"/> if <paramref name="item"/> is found in the <see cref="ConcurrentList{T}"/>;
        /// otherwise, <see langword="false"/>.</returns>
        public bool Contains(T item) {
            ConcurrentListSegment<T>? segment = head;
            do {
                if (segment.Contains(item))
                    return true;
                segment = segment.Previous;
            } while (segment is not null);

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
            foreach (T element in this)
                array[arrayIndex++] = element;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="ConcurrentList{T}"/>.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the <see cref="ConcurrentList{T}"/>.</returns>
        public IEnumerator<T> GetEnumerator() {
            ConcurrentListSegment<T>? segment = head;
            do {
                foreach (T element in segment)
                    yield return element;
                segment = segment.Previous;
            } while (segment is not null);
        }

        /// <summary>
        /// Removes the first occurrence of a specific <paramref name="item"/> from the <see cref="ConcurrentList{T}"/>.
        /// </summary>
        /// <param name="item">The item to remove from the <see cref="ConcurrentList{T}"/>.</param>
        /// <returns><see langword="true"/> if item was successfully removed from the <see cref="ConcurrentList{T}"/>;
        /// otherwise, <see langword="false"/>. This method also returns <see langword="false"/> if item is not found in
        /// the original <see cref="ConcurrentList{T}"/>.</returns>
        public bool Remove(T item) {
            ConcurrentListSegment<T>? previous = null;
            ConcurrentListSegment<T>? segment = head;

            do {
                if (segment.TryRemove(item)) {
                    if (segment.IsFull && segment.Count == 0 && previous != null)
                        previous.CompareExchangePrevious(segment.Previous, segment);

                    return true;
                }

                previous = segment;
                segment = segment.Previous;
            } while (segment is not null);

            return false;
        }

        private void CreateNextSegmentCompare(ConcurrentListSegment<T> comparand) {
            lock (head) {
                if (head == comparand)
                    head = new ConcurrentListSegment<T>(head, Math.Min(head.Capacity * 2, MaxSegmentCapacity));
            }
        }

        void ICollection.CopyTo(Array array, int index) {
            foreach (T element in this)
                array.SetValue(element, index++);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

    }
}
