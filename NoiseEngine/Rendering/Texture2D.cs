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
        GraphicsDevice device, TextureUsage usage, uint width, uint height,
        TextureFormat format = TextureFormat.R8G8B8A8_SRGB, uint mipLevels = 1, bool linear = false,
        uint sampleCount = 1
    ) : base(device, usage, format, TextureHelper.CreateHandle(device, new TextureCreateInfo(
        new Vector3<uint>(width, height, 1), format, mipLevels, 1, sampleCount, linear, usage, true,
        new VulkanImageCreateInfo(
            0, VulkanImageType.Image2D, VulkanImageLayout.Undefined
        )), out InteropHandle<Texture> innerHandle
    ), innerHandle) {
        Width = width;
        Height = height;
        MipLevels = mipLevels;
        SampleCount = sampleCount;
    }

    /// <summary>
    /// Creates a PNG file data from this <see cref="Texture2D"/>.
    /// </summary>
    /// <returns>File data.</returns>
    public byte[] ToPng() {
        return CpuTexture2D.FromTexture2D(this).ToPng();
    }

    /// <summary>
    /// Creates a JPEG file data from this <see cref="Texture2D"/>.
    /// </summary>
    /// <param name="quality">Quality of the compression between 0 and 100.</param>
    /// <returns>File data.</returns>
    public byte[] ToJpeg(byte quality = 75) {
        return CpuTexture2D.FromTexture2D(this).ToJpeg(quality);
    }

    /// <summary>
    /// Creates a WebP file data from this <see cref="Texture2D"/>.
    /// </summary>
    /// <param name="quality">
    /// Quality of the compression between 0 and 100.
    /// Value of null is lossless compression.
    /// </param>
    /// <returns>File data.</returns>
    public byte[] ToWebP(byte? quality = null) {
        return CpuTexture2D.FromTexture2D(this).ToWebP(quality);
    }

}
