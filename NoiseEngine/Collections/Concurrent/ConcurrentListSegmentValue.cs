namespace NoiseEngine.Collections.Concurrent;

internal readonly record struct ConcurrentListSegmentValue<T>(T? Value, bool HasValue) {

    public ConcurrentListSegmentValue(T value) : this(value, true) {
    }

}
