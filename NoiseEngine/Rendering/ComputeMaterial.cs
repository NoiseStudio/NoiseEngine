using NoiseEngine.Nesl;

namespace NoiseEngine.Rendering;

public class ComputeMaterial : CommonMaterial {

    public ComputeShader Shader { get; }

    public ComputeMaterial(ComputeShader shader) : base(shader) {
        Shader = shader;
    }

    /// <summary>
    /// Tries return <see cref="ComputeKernel"/> created from given <paramref name="neslMethod"/>.
    /// </summary>
    /// <param name="neslMethod">Origin <see cref="NeslMethod"/> of returned <see cref="ComputeKernel"/>.</param>
    /// <returns>
    /// <see cref="ComputeKernel"/> when this <see cref="ComputeShader"/> contains given
    /// <paramref name="neslMethod"/>; otherwise null.
    /// </returns>
    public ComputeKernel? GetKernel(NeslMethod neslMethod) {
        return Shader.GetKernel(neslMethod);
    }

}
