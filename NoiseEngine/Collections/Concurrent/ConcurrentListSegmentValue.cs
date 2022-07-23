namespace NoiseEngine.Collections.Concurrent;

internal readonly struct ConcurrentListSegmentValue<T> {

    public T? Value { get; }
    public bool HasValue { get; }

    public ConcurrentListSegmentValue(T value) {
        Value = value;
        HasValue = true;
    }

}
