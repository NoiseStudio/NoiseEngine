using NoiseEngine.Nesl;
using NoiseEngine.Rendering.Exceptions;
using NoiseEngine.Rendering.Vulkan;

namespace NoiseEngine.Rendering;

public class Shader : ICommonShader {

    public GraphicsDevice Device { get; }
    public NeslType ClassData { get; }

    internal CommonShaderDelegation Delegation { get; }

    public Shader(GraphicsDevice device, NeslType classData, ShaderSettings settings) {
        device.Initialize();

        Device = device;
        ClassData = classData;

        Delegation = device.Instance.Api switch {
            GraphicsApi.Vulkan => new VulkanCommonShaderDelegation(this, settings),
            _ => throw new GraphicsApiNotSupportedException(device.Instance.Api),
        };
    }

    public Shader(GraphicsDevice device, NeslType classData) : this(
        device, classData, ShaderSettings.Performance
    ) {
    }

}
