using NoiseEngine.Nesl;
using NoiseEngine.Rendering;

namespace NoiseEngine.Common.Shaders;

internal interface ICommonShader {

    public GraphicsDevice Device { get; }
    public NeslType ClassData { get; }

}
