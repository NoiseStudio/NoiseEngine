namespace NoiseEngine.Rendering;

public class Material {

    public Shader Shader { get; }

    public Material(Shader shader) {
        Shader = shader;
    }

}
