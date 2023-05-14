namespace NoiseEngine.Rendering;

internal abstract class CommonMaterialDelegation {

    public ICommonShader Shader { get; }

    protected CommonMaterialDelegation(ICommonShader shader) {
        Shader = shader;
    }

}
