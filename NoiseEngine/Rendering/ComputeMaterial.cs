namespace NoiseEngine.Rendering;

public class ComputeMaterial : CommonMaterial {

    public ComputeShader Shader { get; }

    public ComputeMaterial(ComputeShader shader) : base(shader) {
        Shader = shader;
    }

}
