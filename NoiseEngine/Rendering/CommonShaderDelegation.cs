namespace NoiseEngine.Rendering;

internal abstract class CommonShaderDelegation {

    public ICommonShader Shader { get; }

    internal bool IsCompute => Shader is ComputeShader;

    protected CommonShaderDelegation(ICommonShader shader) {
        Shader = shader;
    }

}
