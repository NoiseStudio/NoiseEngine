namespace NoiseEngine.Rendering;

public readonly record struct ShaderSettings(
    ShaderRoundingMode RoundingMode
) {

    public static ShaderSettings Conformant => new ShaderSettings(ShaderRoundingMode.ToEven);
    public static ShaderSettings Performance => new ShaderSettings(ShaderRoundingMode.Any);

}
