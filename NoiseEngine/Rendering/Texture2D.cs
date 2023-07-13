using System;
using System.Threading;
using NoiseEngine.Interop;
using NoiseEngine.Mathematics;
using NoiseEngine.Rendering.Vulkan;

namespace NoiseEngine.Rendering;

public class Texture2D : Texture {

    private VulkanImageView? vulkanDefaultImageView;

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
            0, VulkanImageType.Image2D, VulkanImageLayout.General
        )), out InteropHandle<Texture> innerHandle
    ), innerHandle) {
        Width = width;
        Height = height;
        MipLevels = mipLevels;
        SampleCount = sampleCount;
    }

    /// <summary>
    /// Decodes given <paramref name="fileData"/> to <see cref="CpuTexture2D"/>.
    /// Makes an educated guess about file format.
    /// </summary>
    /// <param name="fileData">File data.</param>
    /// <param name="device"><see cref="GraphicsDevice"/> to use.</param>
    /// <param name="usage">
    /// Texture usage flags. Must contain <see cref="TextureUsage.TransferDestination"/> flag
    /// to allow for copying CPU data into it.
    /// </param>
    /// <param name="format">Target format for the texture. Null tries to use the format of the file.</param>
    /// <param name="mipLevels">Mipmap levels.</param>
    /// <param name="linear">Use linear memory layout instead of optimal.</param>
    /// <param name="sampleCount">Samples per texel.</param>
    /// <returns>Created <see cref="Texture2D"/>.</returns>
    /// <returns>Result texture.</returns>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="usage"/> does not have a <see cref="TextureUsage.TransferDestination"/> flag.
    /// </exception>
    /// <exception cref="ArgumentException">Throws if decoding file data fails.</exception>
    public static Texture2D FromFile(
        ReadOnlySpan<byte> fileData,
        GraphicsDevice device,
        TextureUsage usage,
        TextureFormat? format = null,
        uint mipLevels = 1,
        bool linear = false,
        uint sampleCount = 1
    ) {
        return CpuTexture2D.FromFile(fileData, format).ToTexture2D(device, usage, mipLevels, linear, sampleCount);
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

    internal override VulkanImageView GetVulkanDefaultImageView() {
        VulkanImageView? view = vulkanDefaultImageView;
        if (view is not null)
            return view;

        Interlocked.CompareExchange(ref vulkanDefaultImageView, new VulkanImageView(this, new VulkanImageViewCreateInfo(
            Handle, 0, VulkanImageViewType.Type2D,
            new ComponentMapping(
                ComponentSwizzle.Identity, ComponentSwizzle.Identity, ComponentSwizzle.Identity,
                ComponentSwizzle.Identity
            ),
            VulkanImageAspect.Color, 0, MipLevels, 0, 1
        )), null);

        return vulkanDefaultImageView;
    }

}
