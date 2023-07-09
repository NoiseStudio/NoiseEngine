namespace NoiseEngine.Rendering;

public class TextureSamplerBuilder {

    public float MaxAnisotropy { get; set; }

    /// <summary>
    /// Builds a <see cref="TextureSampler"/> from this <see cref="TextureSamplerBuilder"/> or returns existing
    /// <see cref="TextureSampler"/> with identical settings corrected by limits of <paramref name="device"/>.
    /// </summary>
    /// <param name="device"><see cref="GraphicsDevice"/> of returned <see cref="TextureSampler"/>.</param>
    /// <returns>
    /// <see cref="TextureSampler"/> with settings from this <see cref="TextureSamplerBuilder"/> corrected by limits of
    /// <paramref name="device"/>.
    /// </returns>
    public TextureSampler Build(GraphicsDevice device) {
        return new TextureSampler(device, MaxAnisotropy);
    }

}
