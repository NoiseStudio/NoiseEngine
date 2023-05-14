namespace NoiseEngine.Rendering;

public class Material : CommonMaterial {

    public Shader Shader { get; }

    public Material(Shader shader) : base(shader) {
        Shader = shader;
    }

}
