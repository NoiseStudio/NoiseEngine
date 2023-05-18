namespace NoiseEngine.Serialization;

public interface ISerializable<T> : ISerializable where T : ISerializable<T> {

    /// <summary>
    /// Creates new T with data from <paramref name="reader"/>.
    /// </summary>
    /// <param name="reader"><see cref="SerializationReader"/>.</param>
    /// <returns>New T with data from <paramref name="reader"/>.</returns>
    public static abstract new T Deserialize(SerializationReader reader);

    static ISerializable ISerializable.Deserialize(SerializationReader reader) {
        return T.Deserialize(reader);
    }

}
