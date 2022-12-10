using NoiseEngine.Nesl;
using NoiseEngine.Rendering.Exceptions;
using NoiseEngine.Rendering.Vulkan;

namespace NoiseEngine.Rendering;

public class ComputeShader : ICommonShader {

    public GraphicsDevice Device { get; }
    public NeslType ClassData { get; }

    internal CommonShaderDelegation Delegation { get; }

    public ComputeShader(GraphicsDevice device, NeslType classData) {
        Device = device;
        ClassData = classData;

        Delegation = device.Instance.Api switch {
            GraphicsApi.Vulkan => new VulkanCommonShaderDelegation(this),
            _ => throw new GraphicsApiNotSupportedException(device.Instance.Api),
        };
    }

    private ComputeShader(GraphicsDevice device, NeslType classData, CommonShaderDelegation delegation) {
        Device = device;
        ClassData = classData;
        Delegation = delegation.Clone(this);
    }

    /// <summary>
    /// Tries return <see cref="ShaderProperty"/> created from given <paramref name="neslProperty"/>.
    /// </summary>
    /// <param name="neslProperty">Origin <see cref="NeslField"/> of returned <see cref="ShaderProperty"/>.</param>
    /// <returns>
    /// <see cref="ShaderProperty"/> when this <see cref="ComputeShader"/> contains given
    /// <paramref name="neslProperty"/>; otherwise null.
    /// </returns>
    public ShaderProperty? GetProperty(NeslField neslProperty) {
        return Delegation.Properties.TryGetValue(neslProperty, out ShaderProperty? property) ? property : null;
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
        return Delegation.Kernels!.TryGetValue(neslMethod, out ComputeKernel? kernel) ? kernel : null;
    }

    /// <summary>
    /// Clones this <see cref="ComputeShader"/>.
    /// </summary>
    /// <returns>Clone of this <see cref="ComputeShader"/> with no properties specified.</returns>
    public ComputeShader Clone() {
        return new ComputeShader(Device, ClassData, Delegation);
    }

}
