namespace NoiseEngine.Serialization;

public interface ISerializable {

    /// <summary>
    /// Creates new object with data from <paramref name="reader"/>.
    /// </summary>
    /// <param name="reader"><see cref="SerializationReader"/>.</param>
    /// <returns>New object with data from <paramref name="reader"/>.</returns>
    public static abstract ISerializable Deserialize(SerializationReader reader);

    /// <summary>
    /// Serializes this object and writes it to the <paramref name="writer"/>.
    /// </summary>
    /// <param name="writer"><see cref="SerializationWriter"/>.</param>
    public void Serialize(SerializationWriter writer);

}
