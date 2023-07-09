using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering;
using NoiseEngine.Interop.Rendering.Vulkan;
using NoiseEngine.Rendering.Exceptions;

namespace NoiseEngine.Rendering;

public class TextureSampler {

    public GraphicsDevice Device { get; }
    public float MaxAnisotropy { get; }

    internal InteropHandle<TextureSampler> Handle { get; }
    internal InteropHandle<TextureSampler> InnerHandle { get; }

    internal TextureSampler(GraphicsDevice device, float maxAnisotropy) {
        device.Initialize();

        Device = device;
        MaxAnisotropy = maxAnisotropy;

        TextureSamplerCreateInfo createInfo = new TextureSamplerCreateInfo(MaxAnisotropy);
        InteropResult<TextureSamplerCreateReturnValue> result = device.Instance.Api switch {
            GraphicsApi.Vulkan => VulkanTextureSamplerInterop.Create(device.Handle, createInfo),
            _ => throw new GraphicsApiNotSupportedException(device.Instance.Api),
        };

        if (!result.TryGetValue(out TextureSamplerCreateReturnValue value, out ResultError error))
            error.ThrowAndDispose();

        Handle = value.Handle;
        InnerHandle = value.InnerHandle;
    }

    ~TextureSampler() {
        if (Handle == InteropHandle<TextureSampler>.Zero)
            return;

        TextureSamplerInterop.Destroy(Handle);
    }

}
