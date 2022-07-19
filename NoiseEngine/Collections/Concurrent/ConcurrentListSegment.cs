using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace NoiseEngine.Collections.Concurrent;

internal class ConcurrentListSegment<T> : IEnumerable<T> {

    private readonly ConcurrentListSegmentValue<T>[] items;

    private ConcurrentListSegment<T>? previous;

    private int nextIndex;
    private int count;

    public int Capacity => items.Length;
    public int Count => count;
    public bool IsFull => nextIndex >= Capacity - 1;

    public ConcurrentListSegment<T>? Previous => previous;

    public ConcurrentListSegment(ConcurrentListSegment<T>? previous, ConcurrentListSegmentValue<T>[] items) {
        this.previous = previous;
        this.items = items;
    }

    public ConcurrentListSegment(ConcurrentListSegment<T>? previous, int capacity)
        : this(previous, new ConcurrentListSegmentValue<T>[capacity]) {
    }

    public void CompareExchangePrevious(ConcurrentListSegment<T>? value, ConcurrentListSegment<T>? comparand) {
        Interlocked.CompareExchange(ref previous, value, comparand);
    }

    public bool TryAdd(T item) {
        int index = Interlocked.Increment(ref nextIndex);
        if (index >= Capacity) {
            Interlocked.Decrement(ref nextIndex);
            return false;
        }

        items[index - 1] = new ConcurrentListSegmentValue<T>(item);
        Interlocked.Increment(ref count);
        return true;
    }

    public bool TryRemove(T item) {
        int max = Math.Min(nextIndex, Capacity);
        for (int i = 0; i < max; i++) {
            ConcurrentListSegmentValue<T> element = items[i];
            if (!element.HasValue || !element.Value!.Equals(item))
                continue;

            items[i] = default;
            Interlocked.Decrement(ref count);
            return true;
        }

        return false;
    }

    public IEnumerator<T> GetEnumerator() {
        int max = Math.Min(nextIndex, Capacity);
        for (int i = 0; i < max; i++) {
            ConcurrentListSegmentValue<T> element = items[i];
            if (element.HasValue)
                yield return element.Value!;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

}
