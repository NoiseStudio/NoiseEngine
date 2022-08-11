namespace NoiseEngine.Nesl;

internal class SerializedNeslAttribute : NeslAttribute {

    /// <summary>
    /// Checks if that properties have valid values.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when attribute properties are valid; otherwise <see langword="false"/>.
    /// </returns>
    public override bool CheckIsValid() {
        return true;
    }

}
