using NoiseEngine.Interop;
using NoiseEngine.Mathematics;
using NoiseEngine.Rendering.Vulkan;

namespace NoiseEngine.Rendering;

public class Texture2D : Texture {

    public uint Width { get; }
    public uint Height { get; }
    public uint MipLevels { get; }
    public uint SampleCount { get; }

    internal override Vector3<uint> Extent => new Vector3<uint>(Width, Height, 1);
    internal override uint SampleCountInternal => SampleCount;

    public Texture2D(
        GraphicsDevice device, uint width, uint height, TextureFormat format = TextureFormat.R8G8B8A8_SRGB,
        uint mipLevels = 1, bool linear = false, uint sampleCount = 1
    ) : base(device, format, TextureHelper.CreateHandle(device, new TextureCreateInfo(
        new Vector3<uint>(width, height, 1), format, mipLevels, 1, sampleCount, linear, TextureUsage.TransferAll | TextureUsage.ColorAttachment,
        true, new VulkanImageCreateInfo(
            0, VulkanImageType.Image2D, VulkanImageLayout.Undefined
        )), out InteropHandle<Texture> innerHandle
    ), innerHandle) {
        Width = width;
        Height = height;
        MipLevels = mipLevels;
        SampleCount = sampleCount;
    }

}
