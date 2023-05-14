using NoiseEngine.Nesl;

namespace NoiseEngine.Rendering;

internal interface ICommonShader {

    public GraphicsDevice Device { get; }
    public NeslType ClassData { get; }
    public ShaderType Type { get; }

}
