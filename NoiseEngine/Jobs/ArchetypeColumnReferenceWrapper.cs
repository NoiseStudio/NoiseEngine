namespace NoiseEngine.Jobs;

internal readonly struct ArchetypeColumnReferenceWrapper<T> {

    public readonly T Value;

    public ArchetypeColumnReferenceWrapper(T value) {
        Value = value;
    }

}
