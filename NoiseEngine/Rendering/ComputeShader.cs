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

    public ShaderProperty? GetProperty(NeslField neslProperty) {
        return Delegation.Properties.TryGetValue(neslProperty, out ShaderProperty? property) ? property : null;
    }

    public ComputeKernel? GetKernel(NeslMethod neslMethod) {
        return Delegation.Kernels!.TryGetValue(neslMethod, out ComputeKernel? kernel) ? kernel : null;
    }

}
