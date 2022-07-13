using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NoiseEngine.Collections;

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

    /// <summary>
    /// Adds <paramref name="item"/> to the <see cref="FastList{T}"/>.
    /// </summary>
    /// <param name="item">Item to add.</param>
    public void Add(T item) {
        EnsureCapacity(count + 1);
        UnsafeAdd(item);
    }

    /// <summary>
    /// Adds <paramref name="item"/> to the <see cref="FastList{T}"/> without ensuring capacity.
    /// </summary>
    /// <param name="item">Item to add.</param>
    public void UnsafeAdd(T item) {
        items[count++] = item;
    }

    /// <summary>
    /// Adds <paramref name="items"/> to the <see cref="FastList{T}"/>.
    /// </summary>
    /// <param name="items">Items to add.</param>
    public void AddRange(ICollection<T>[] items) {
        EnsureCapacity(count + items.Length);
        items.CopyTo(this.items, count);
        count += items.Length;
    }

    /// <summary>
    /// Adds <paramref name="items"/> to the <see cref="FastList{T}"/>.
    /// </summary>
    /// <param name="items">Items to add.</param>
    public void AddRange(Span<T> items) {
        EnsureCapacity(count + items.Length);
        items.CopyTo(this.items.AsSpan(count, items.Length));
        count += items.Length;
    }

    /// <summary>
    /// Removes all elements from the <see cref="FastList{T}"/>.
    /// </summary>
    public void Clear() {
        if (count == 0)
            return;

        Array.Clear(items, 0, count);
        count = 0;
    }

    /// <summary>
    /// Determines whether the <see cref="FastList{T}"/> contains specific value.
    /// </summary>
    /// <param name="item">The item to locate in the <see cref="FastList{T}"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="item"/> is found in the <see cref="FastList{T}"/>;
    /// otherwise, <see langword="false"/>.</returns>
    public bool Contains(T item) {
        for (int i = 0; i < count; i++) {
            if (items[i]!.Equals(item))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Copies the elements of the <see cref="FastList{T}"/> to an <paramref name="array"/>,
    /// starting at a particular <paramref name="array"/> index.
    /// </summary>
    /// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the elements copied
    /// from <see cref="FastList{T}"/>. The <see cref="Array"/> must have zero-based indexing.</param>
    /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
    public void CopyTo(T[] array, int arrayIndex) {
        items.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Copies the elements of the <see cref="FastList{T}"/> to an <paramref name="list"/>,
    /// starting at a particular <paramref name="list"/> index.
    /// </summary>
    /// <param name="list">The one-dimensional <see cref="FastList{T}"/> that is the destination of the elements
    /// copied from <see cref="FastList{T}"/>. The <see cref="FastList{T}"/> must have zero-based indexing.</param>
    /// <param name="listIndex">The zero-based index in <paramref name="list"/> at which copying begins.</param>
    public void CopyTo(FastList<T> list, int listIndex) {
        int newCount = listIndex + count;

        if (newCount > list.count) {
            list.EnsureCapacity(newCount);
            list.count = newCount;
        }

        items.CopyTo(list.items, listIndex);
    }

    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="FastList{T}"/>.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the <see cref="FastList{T}"/>.</returns>
    public IEnumerator<T> GetEnumerator() {
        for (int i = 0; i < count; i++)
            yield return items[i];
    }

    /// <summary>
    /// Determines the index of a specific <paramref name="item"/> in the <see cref="FastList{T}"/>.
    /// </summary>
    /// <param name="item">The item to locate in the <see cref="FastList{T}"/>.</param>
    /// <returns>The index of item if found in the <see cref="FastList{T}"/>; otherwise, -1.</returns>
    public int IndexOf(T item) {
        for (int i = 0; i < count; i++) {
            if (items[i]!.Equals(item))
                return i;
        }

        return -1;
    }

    /// <summary>
    /// Inserts an item to the <see cref="FastList{T}"/> at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
    /// <param name="item">The item to insert into the <see cref="FastList{T}"/>.</param>
    public void Insert(int index, T item) {
        MoveElements(index, 1);
        items[index] = item;
        count++;
    }

    /// <summary>
    /// Inserts an items to the <see cref="FastList{T}"/> at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which <paramref name="items"/> should be inserted.</param>
    /// <param name="items">The item to insert into the <see cref="FastList{T}"/>.</param>
    public void InsertRange(int index, ICollection<T>[] items) {
        MoveElements(index, items.Length);
        items.CopyTo(this.items, index);
        count += items.Length;
    }

    /// <summary>
    /// Inserts an items to the <see cref="FastList{T}"/> at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which <paramref name="items"/> should be inserted.</param>
    /// <param name="items">The item to insert into the <see cref="FastList{T}"/>.</param>
    public void InsertRange(int index, Span<T> items) {
        MoveElements(index, items.Length);
        items.CopyTo(AsSpan(index, items.Length));
        count += items.Length;
    }

    /// <summary>
    /// Removes the first occurrence of a specific <paramref name="item"/> from the <see cref="List{T}"/>.
    /// </summary>
    /// <param name="item">The item to remove from the <see cref="List{T}"/>.</param>
    /// <returns><see langword="true"/> if item was successfully removed from the <see cref="List{T}"/>;
    /// otherwise, <see langword="false"/>. This method also returns <see langword="false"/> if item is not found in
    /// the original <see cref="List{T}"/>.</returns>
    public bool Remove(T item) {
        for (int i = 0; i < count; i++) {
            if (items[i]!.Equals(item)) {
                RemoveAt(i);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Removes the <see cref="FastList{T}"/> item at the specified <paramref name="index"/>.
    /// </summary>
    /// <param name="index">The zero-based index of the item to remove.</param>
    public void RemoveAt(int index) {
        int indexP = index + 1;
        AsSpan(indexP, count - indexP).CopyTo(AsSpan(index, count - index));
        items[--count] = default!;
    }

    /// <summary>
    /// Copies the elements of the <see cref="FastList{T}"/> to a new array.
    /// </summary>
    /// <returns>An array containing copies of the elements of the <see cref="FastList{T}"/>.</returns>
    public T[] ToArray() {
        T[] result = new T[count];
        Array.Copy(items, result, count);
        return result;
    }

    /// <summary>
    /// Creates a new span over this <see cref="FastList{T}"/>.
    /// </summary>
    /// <returns>The span representation of the <see cref="FastList{T}"/>.</returns>
    public Span<T> AsSpan() {
        return items.AsSpan(0, count);
    }

    /// <summary>
    /// Creates a new span over a portion of this <see cref="FastList{T}"/> starting at a specified
    /// position to the end of the <see cref="FastList{T}"/>.
    /// </summary>
    /// <param name="start">The initial index from which the <see cref="FastList{T}"/> will be converted.</param>
    /// <returns>The span representation of the <see cref="FastList{T}"/>.</returns>
    public Span<T> AsSpan(int start) {
        return items.AsSpan(start, count - start);
    }

    /// <summary>
    /// Creates a new span over a portion of this <see cref="FastList{T}"/> starting at a specified
    /// position for a specified length.
    /// </summary>
    /// <param name="start">The initial index from which the <see cref="FastList{T}"/> will be converted.</param>
    /// <param name="length">The number of items in the span.</param>
    /// <returns>The span representation of the <see cref="FastList{T}"/>.</returns>
    public Span<T> AsSpan(int start, int length) {
        return items.AsSpan(start, length);
    }

    /// <summary>
    /// Sets the <see cref="Capacity"/> to the actual number of elements in the <see cref="FastList{T}"/>,
    /// if that number is less than a threshold value.
    /// </summary>
    public void TrimExcess() {
        if (count < Capacity * 0.9f)
            Array.Resize(ref items, count);
    }

    /// <summary>
    /// Ensures that the <see cref="Capacity"/> is at least the given <paramref name="minCapacity"/>.
    /// If current <see cref="Capacity"/> is less than <paramref name="minCapacity"/>, <see cref="Capacity"/>
    /// is increased to twice the <see cref="Capacity"/>.
    /// </summary>
    /// <param name="minCapacity"></param>
    public void EnsureCapacity(int minCapacity) {
        if (minCapacity <= Capacity)
            return;

        int newCapacity = Capacity == 0 ? DefaultCapacity : Capacity * 2;
        if (newCapacity < minCapacity)
            newCapacity = minCapacity;

        Array.Resize(ref items, newCapacity);
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

    private void MoveElements(int index, int move) {
        EnsureCapacity(count + move);

        int a = count - index;
        AsSpan(index, a).CopyTo(AsSpan(index + move, a));
    }

}
