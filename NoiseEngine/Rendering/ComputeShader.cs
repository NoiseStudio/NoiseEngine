using NoiseEngine.Nesl;
using NoiseEngine.Rendering.Exceptions;
using NoiseEngine.Rendering.Vulkan;
using System.Collections.Generic;

namespace NoiseEngine.Rendering;

public class ComputeShader : ICommonShader {

    private readonly Dictionary<NeslMethod, ComputeKernel> kernels;

    public GraphicsDevice Device { get; }
    public NeslType ClassData { get; }

    internal CommonShaderDelegation Delegation { get; }

    ShaderType ICommonShader.Type => ShaderType.Compute;
    CommonShaderDelegation ICommonShader.Delegation => Delegation;

    public ComputeShader(GraphicsDevice device, NeslType classData, ShaderSettings settings) {
        Device = device;
        ClassData = classData;

        switch (device.Instance.Api) {
            case GraphicsApi.Vulkan:
                VulkanComputeShaderDelegation d = new VulkanComputeShaderDelegation(this, settings);
                kernels = d.Kernels;
                Delegation = d;
                break;
            default:
                throw new GraphicsApiNotSupportedException(device.Instance.Api);
        }
    }

    public ComputeShader(GraphicsDevice device, NeslType classData) : this(
        device, classData, ShaderSettings.Conformant
    ) {
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
        return kernels.TryGetValue(neslMethod, out ComputeKernel? kernel) ? kernel : null;
    }

}
