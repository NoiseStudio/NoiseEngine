using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace NoiseEngine.Collections.Concurrent;

public class ConcurrentHashSet<T> : IEnumerable<T>, ISet<T> where T : notnull {

    private readonly ConcurrentDictionary<T, byte> storage = new ConcurrentDictionary<T, byte>();

    public int Count => storage.Count;
    public bool IsReadOnly => false;

    public ConcurrentHashSet() {
    }

    public ConcurrentHashSet(IEnumerable<T> enumerable) {
        foreach (T element in enumerable)
            Add(element);
    }

    /// <summary>
    /// Adds an element to the <see cref="ConcurrentHashSet{T}"/> and returns a value to indicate
    /// if the element was successfully added.
    /// </summary>
    /// <param name="item">The element to add to the <see cref="ConcurrentHashSet{T}"/>.</param>
    /// <returns><see langword="true"/> if the element is added to the <see cref="ConcurrentHashSet{T}"/>;
    /// <see langword="false"/> if the element is already in the <see cref="ConcurrentHashSet{T}"/>.</returns>
    public bool Add(T item) {
        return storage.TryAdd(item, 0);
    }

    /// <summary>
    /// Removes the first occurrence of a specific object from the <see cref="ConcurrentHashSet{T}"/>.
    /// </summary>
    /// <param name="item">The object to remove from the <see cref="ConcurrentHashSet{T}"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="item"/> was successfully removed from the
    /// <see cref="ConcurrentHashSet{T}"/>; otherwise, <see langword="false"/>. This method also returns
    /// <see langword="false"/> if <paramref name="item"/> is not found in the original
    /// <see cref="ConcurrentHashSet{T}"/>.</returns>
    public bool Remove(T item) {
        return storage.TryRemove(item, out _);
    }

    /// <summary>
    /// Removes all items from the <see cref="ConcurrentHashSet{T}"/>.
    /// </summary>
    public void Clear() {
        storage.Clear();
    }

    /// <summary>
    /// Determines whether the <see cref="ConcurrentHashSet{T}"/> contains a specific value.
    /// </summary>
    /// <param name="item">The object to locate in the <see cref="ConcurrentHashSet{T}"/>.</param>
    /// <returns><see langword="true"/> if item is found in the <see cref="ConcurrentHashSet{T}"/>;
    /// otherwise, <see langword="false"/>.</returns>
    public bool Contains(T item) {
        return storage.ContainsKey(item);
    }

    /// <summary>
    /// Copies the elements of the <see cref="ConcurrentHashSet{T}"/> to an <see cref="Array"/>,
    /// starting at a particular <paramref name="arrayIndex"/>.
    /// </summary>
    /// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the elements copied
    /// from <see cref="ConcurrentHashSet{T}"/>. The <see cref="Array"/> must have zero-based indexing.</param>
    /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
    public void CopyTo(T[] array, int arrayIndex) {
        foreach (T value in this)
            array[arrayIndex++] = value;
    }

    /// <summary>
    /// Removes all elements in the specified collection from the current <see cref="ConcurrentHashSet{T}"/>.
    /// </summary>
    /// <param name="other">The collection of items to remove from the set.</param>
    public void ExceptWith(IEnumerable<T> other) {
        if (Count == 0)
            return;

        if (other == this) {
            Clear();
            return;
        }

        foreach (T element in other)
            Remove(element);
    }

    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="ConcurrentHashSet{T}"/>.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the <see cref="ConcurrentHashSet{T}"/>.</returns>
    public IEnumerator<T> GetEnumerator() {
        foreach (T value in storage.Keys)
            yield return value;
    }

    /// <summary>
    /// Modifies the current <see cref="ConcurrentHashSet{T}"/> so that it
    /// contains only elements that are also in a specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current <see cref="ConcurrentHashSet{T}"/>.</param>
    public void IntersectWith(IEnumerable<T> other) {
        if (Count == 0)
            return;

        if (other is ICollection<T> collection) {
            if (collection.Count == 0) {
                Clear();
                return;
            }
        } else {
            collection = new HashSet<T>(other);
        }

        foreach (T element in this) {
            if (!collection.Contains(element))
                Remove(element);
        }
    }

    /// <summary>
    /// Determines whether the current <see cref="ConcurrentHashSet{T}"/> overlaps with the specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current <see cref="ConcurrentHashSet{T}"/>.</param>
    /// <returns><see langword="true"/> if the current <see cref="ConcurrentHashSet{T}"/> and
    /// <paramref name="other"/> share at least one common element; otherwise, <see langword="false"/>.</returns>
    public bool Overlaps(IEnumerable<T> other) {
        if (Count == 0)
            return false;

        foreach (T element in other) {
            if (Contains(element))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Determines whether the current <see cref="ConcurrentHashSet{T}"/> and
    /// the specified collection contain the same elements.
    /// </summary>
    /// <param name="other">The collection to compare to the current <see cref="ConcurrentHashSet{T}"/>.</param>
    /// <returns><see langword="true"/> if the current <see cref="ConcurrentHashSet{T}"/> is equal to other;
    /// otherwise, <see langword="false"/>.</returns>
    public bool SetEquals(IEnumerable<T> other) {
        int count = 0;

        foreach (T item in other) {
            if (!Contains(item))
                return false;
            count++;
        }

        return Count == count;
    }

    /// <summary>
    /// Modifies the current <see cref="ConcurrentHashSet{T}"/> so that it contains only elements that are present
    /// either in the current <see cref="ConcurrentHashSet{T}"/> or in the specified collection, but not both.
    /// </summary>
    /// <param name="other">The collection to compare to the current <see cref="ConcurrentHashSet{T}"/>.</param>
    public void SymmetricExceptWith(IEnumerable<T> other) {
        if (Count == 0) {
            UnionWith(other);
            return;
        }

        if (other == this) {
            Clear();
            return;
        }

        foreach (T element in other.Distinct()) {
            if (!Remove(element))
                Add(element);
        }
    }

    /// <summary>
    /// Modifies the current <see cref="ConcurrentHashSet{T}"/> so that it contains all elements that are present
    /// in the current <see cref="ConcurrentHashSet{T}"/>, in the specified collection, or in both.
    /// </summary>
    /// <param name="other">The collection to compare to the current <see cref="ConcurrentHashSet{T}"/>.</param>
    public void UnionWith(IEnumerable<T> other) {
        foreach (T item in other)
            Add(item);
    }

    bool ISet<T>.IsProperSubsetOf(IEnumerable<T> other) {
        throw new NotImplementedException();
    }

    bool ISet<T>.IsProperSupersetOf(IEnumerable<T> other) {
        throw new NotImplementedException();
    }

    bool ISet<T>.IsSubsetOf(IEnumerable<T> other) {
        throw new NotImplementedException();
    }

    bool ISet<T>.IsSupersetOf(IEnumerable<T> other) {
        throw new NotImplementedException();
    }

    void ICollection<T>.Add(T item) {
        Add(item);
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

}
