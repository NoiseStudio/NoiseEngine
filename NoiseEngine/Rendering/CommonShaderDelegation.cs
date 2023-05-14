using NoiseEngine.Nesl;

namespace NoiseEngine.Rendering;

internal abstract class CommonShaderDelegation {

    public ICommonShader Shader { get; }
    public (NeslField, MaterialProperty)[]? Properties { get; protected set; }

    protected CommonShaderDelegation(ICommonShader shader) {
        Shader = shader;
    }

}
