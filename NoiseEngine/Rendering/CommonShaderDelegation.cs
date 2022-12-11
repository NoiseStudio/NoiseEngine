using NoiseEngine.Nesl;
using System.Collections.Generic;

namespace NoiseEngine.Rendering;

internal abstract class CommonShaderDelegation {

    public ICommonShader Shader { get; }

    internal Dictionary<NeslField, ShaderProperty> Properties { get; } =
        new Dictionary<NeslField, ShaderProperty>();
    internal Dictionary<NeslMethod, ComputeKernel>? Kernels { get; private protected set; }

    internal bool IsCompute => Shader is ComputeShader;

    protected CommonShaderDelegation(ICommonShader shader) {
        Shader = shader;
    }

    internal abstract CommonShaderDelegation Clone(ICommonShader newShader);

}
