using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NoiseEngine.Collections {
    public class FastList<T> : IList<T>, IReadOnlyList<T>, IList {

        private const int DefaultCapacity = 4;

        private T[] items;
        private int count;

        public int Capacity {
            get => items.Length;
            set {
                Array.Resize(ref items, value);
                if (count > Capacity)
                    count = Capacity;
            }
        }

        public int Count => count;

        public T this[int index] {
            get => items[index];
            set => items[index] = value;
        }

        object? IList.this[int index] {
            get => this[index];
            set => this[index] = (T)value!;
        }

        bool ICollection<T>.IsReadOnly => false;
        bool IList.IsFixedSize => false;
        bool IList.IsReadOnly => false;
        bool ICollection.IsSynchronized => false;
        object ICollection.SyncRoot => items;

        public FastList(int capacity = DefaultCapacity) {
            items = new T[capacity];
        }

        public FastList(FastList<T> items) {
            this.items = new T[items.Capacity];
            items.CopyTo(this.items, 0);
            count = items.Count;
        }

        public FastList(T[] items) {
            this.items = new T[items.Length];
            items.CopyTo(this.items, 0);
            count = items.Length;
        }

        public FastList(ICollection<T> items) {
            this.items = new T[items.Count];
            items.CopyTo(this.items, 0);
            count = items.Count;
        }

        public FastList(IEnumerable<T> items) {
            this.items = new T[items.Count()];
            count = this.items.Length;

            int i = 0;
            foreach (T item in items)
                this.items[i++] = item;
        }

        private static bool IsCompatibleObject(object? value) {
            return (value is T) || (value == null && default(T) == null);
        }

        public void Add(T item) {
            EnsureCapacity(count + 1);
            items[count++] = item;
        }

        public void AddRange(ICollection<T>[] items) {
            EnsureCapacity(count + items.Length);
            items.CopyTo(this.items, count);
            count += items.Length;
        }

        public void AddRange(Span<T> items) {
            EnsureCapacity(count + items.Length);
            items.CopyTo(this.items.AsSpan(count, items.Length));
            count += items.Length;
        }

        public void Clear() {
            if (count == 0)
                return;

            Array.Clear(items, 0, count);
            count = 0;
        }

        public bool Contains(T item) {
            for (int i = 0; i < count; i++) {
                if (items[i]!.Equals(item))
                    return true;
            }

            return false;
        }

        public void CopyTo(T[] array, int arrayIndex) {
            items.CopyTo(array, arrayIndex);
        }

        public void CopyTo(FastList<T> list, int listIndex) {
            int newCount = listIndex + count;

            if (newCount > list.count) {
                list.EnsureCapacity(newCount);
                list.count = newCount;
            }

            items.CopyTo(list.items, listIndex);
        }

        public IEnumerator<T> GetEnumerator() {
            for (int i = 0; i < count; i++)
                yield return items[i];
        }

        public int IndexOf(T item) {
            for (int i = 0; i < count; i++) {
                if (items[i]!.Equals(item))
                    return i;
            }

            return -1;
        }

        public void Insert(int index, T item) {
            MoveElements(index, 1);
            items[index] = item;
            count++;
        }

        public void InsertRange(int index, ICollection<T>[] items) {
            MoveElements(index, items.Length);
            items.CopyTo(this.items, index);
            count += items.Length;
        }

        public void InsertRange(int index, Span<T> items) {
            MoveElements(index, items.Length);
            items.CopyTo(AsSpan(index, items.Length));
            count += items.Length;
        }

        public bool Remove(T item) {
            for (int i = 0; i < count; i++) {
                if (items[i]!.Equals(item)) {
                    RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public void RemoveAt(int index) {
            int indexP = index + 1;
            AsSpan(indexP, count - indexP).CopyTo(AsSpan(index, count - index));
            items[--count] = default!;
        }

        public T[] ToArray() {
            T[] result = new T[count];
            Array.Copy(items, result, count);
            return result;
        }

        public Span<T> AsSpan() {
            return items.AsSpan(0, count);
        }

        public Span<T> AsSpan(int start) {
            return items.AsSpan(start, count - start);
        }

        public Span<T> AsSpan(int start, int length) {
            return items.AsSpan(start, length);
        }

        public void TrimExcess() {
            if (count < Capacity * 0.9f)
                Array.Resize(ref items, count);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        void ICollection.CopyTo(Array array, int index) {
            items.CopyTo(array, index);
        }

        int IList.Add(object? value) {
            if (!IsCompatibleObject(value))
                return -1;

            int result = count;
            Add((T)value!);
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

        private void EnsureCapacity(int count) {
            if (count <= this.count)
                return;

            int newCapacity = this.count == 0 ? DefaultCapacity : this.count * 2;
            if (newCapacity < count)
                newCapacity = count;

            Array.Resize(ref items, newCapacity);
        }

        private void MoveElements(int index, int move) {
            EnsureCapacity(count + move);

            int a = count - index;
            AsSpan(index, a).CopyTo(AsSpan(index + move, a));
        }

    }
}
