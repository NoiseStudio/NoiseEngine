namespace NoiseEngine.Rendering;

internal abstract class CommonShaderDelegation {

    public ICommonShader Shader { get; }

    protected CommonShaderDelegation(ICommonShader shader) {
        Shader = shader;
    }

}
