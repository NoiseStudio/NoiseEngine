using NoiseEngine.Nesl;

namespace NoiseEngine.Rendering;

public abstract class ComputeKernel {

    public NeslMethod Method { get; }
    public ComputeShader Shader { get; }

    public string Name => Method.Name;
    public GraphicsDevice Device => Shader.Device;

    private protected ComputeKernel(NeslMethod method, ComputeShader shader) {
        Method = method;
        Shader = shader;
    }

}
