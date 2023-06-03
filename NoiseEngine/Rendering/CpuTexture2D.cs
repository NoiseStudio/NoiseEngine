using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering;
using NoiseEngine.Mathematics;
using NoiseEngine.Rendering.Cpu;
using NoiseEngine.Rendering.Utils;

namespace NoiseEngine.Rendering;

public class CpuTexture2D : CpuTexture {

    private readonly byte[] data;

    public uint Width => Extent.X;
    public uint Height => Extent.Y;

    /// <summary>
    /// Data of the image in row-major order (index = x + Size.X * y).
    /// </summary>
    public override Span<byte> Data => data;

    internal override Vector3<uint> Extent { get; }

    private CpuTexture2D(byte[] data, TextureFormat format, Vector2<uint> size) : base(format) {
        this.data = data;
        Extent = new Vector3<uint>(size.X, size.Y, 1);
    }

    /// <summary>
    /// Tries to decode given <paramref name="fileData"/> to <see cref="CpuTexture2D"/>.
    /// Makes an educated guess about file format.
    /// </summary>
    /// <param name="fileData">File data.</param>
    /// <param name="texture">Result texture.</param>
    /// <param name="format">Target format for the texture. Null tries to use the format of the file.</param>
    /// <returns>True if decoding was successful; otherwise false.</returns>
    public static bool TryFromFile(
        ReadOnlySpan<byte> fileData,
        [NotNullWhen(true)] out CpuTexture2D? texture,
        TextureFormat? format = null
    ) {
        InteropResult<CpuTextureData> result = CpuTextureInterop.Decode(fileData, format);

        if (!result.TryGetValue(out CpuTextureData data, out ResultError error)) {
            error.Dispose();
            texture = null;
            return false;
        }

        Debug.Assert(data.ExtentZ == 1);
        texture = new CpuTexture2D(
            data.Data.AsSpan().ToArray(),
            data.Format,
            new Vector2<uint>(data.ExtentX, data.ExtentY));
        data.Dispose();
        return true;
    }

    /// <summary>
    /// Decodes given <paramref name="fileData"/> to <see cref="CpuTexture2D"/>.
    /// Makes an educated guess about file format.
    /// </summary>
    /// <param name="fileData">File data.</param>
    /// <param name="format">Target format for the texture. Null tries to use the format of the file.</param>
    /// <returns>Result texture.</returns>
    /// <exception cref="ArgumentException">Throws if decoding file data fails.</exception>
    public static CpuTexture2D FromFile(
        ReadOnlySpan<byte> fileData,
        TextureFormat? format = null
    ) {
        InteropResult<CpuTextureData> result = CpuTextureInterop.Decode(fileData, format);

        if (!result.TryGetValue(out CpuTextureData data, out ResultError error)) {
            error.ThrowAndDispose();
        }

        Debug.Assert(data.ExtentZ == 1);
        CpuTexture2D texture = new CpuTexture2D(
            data.Data.AsSpan().ToArray(),
            data.Format,
            new Vector2<uint>(data.ExtentX, data.ExtentY));
        data.Dispose();
        return texture;
    }

    /// <summary>
    /// Creates a <see cref="CpuTexture2D"/> from given <paramref name="texture"/>.
    /// </summary>
    /// <param name="texture">Texture to use data from.</param>
    /// <returns>New <see cref="CpuTexture2D"/>.</returns>
    public static CpuTexture2D FromTexture2D(Texture2D texture) {
        byte[] buffer =
            new byte[(int)texture.Width * (int)texture.Height * TextureFormatUtils.TexelSize(texture.Format)];

        texture.GetPixels(buffer.AsSpan());
        return new CpuTexture2D(buffer, texture.Format, new Vector2<uint>(texture.Width, texture.Height));
    }

    /// <summary>
    /// Converts this <see cref="CpuTexture2D"/> to <see cref="Texture2D"/>.
    /// </summary>
    /// <param name="device"><see cref="GraphicsDevice"/> to use.</param>
    /// <param name="usage">
    /// Texture usage flags. Must contain <see cref="TextureUsage.TransferDestination"/> flag
    /// to allow for copying CPU data into it.
    /// </param>
    /// <param name="mipLevels">Mipmap levels.</param>
    /// <param name="linear">Use linear memory layout instead of optimal.</param>
    /// <param name="sampleCount">Samples per texel.</param>
    /// <returns>Created <see cref="Texture2D"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="usage"/> does not have a <see cref="TextureUsage.TransferDestination"/> flag.
    /// </exception>
    public Texture2D ToTexture2D(
        GraphicsDevice device,
        TextureUsage usage,
        uint mipLevels = 1,
        bool linear = false,
        uint sampleCount = 1
    ) {
        if (!usage.HasFlag(TextureUsage.TransferDestination))
            throw new InvalidOperationException("Usage does not have a TextureUsage.TransferDestination flag.");

        Texture2D texture = new Texture2D(device, usage, Extent.X, Extent.Y, Format, mipLevels, linear, sampleCount);
        texture.SetPixels<byte>(Data);
        return texture;
    }

    /// <summary>
    /// Creates a PNG file data from this <see cref="CpuTexture2D"/>.
    /// </summary>
    /// <returns>File data.</returns>
    public byte[] ToPng() {
        return ToFileData(TextureFileFormat.Png);
    }

    /// <summary>
    /// Creates a JPEG file data from this <see cref="CpuTexture2D"/>.
    /// </summary>
    /// <param name="quality">Quality of the compression between 0 and 100.</param>
    /// <returns>File data.</returns>
    public byte[] ToJpeg(byte quality = 75) {
        return ToFileData(TextureFileFormat.Jpeg, quality);
    }

    /// <summary>
    /// Creates a WebP file data from this <see cref="CpuTexture2D"/>.
    /// </summary>
    /// <param name="quality">
    /// Quality of the compression between 0 and 100.
    /// Value of null is lossless compression.
    /// </param>
    /// <returns>File data.</returns>
    public byte[] ToWebP(byte? quality = null) {
        return ToFileData(TextureFileFormat.WebP, quality);
    }

    private byte[] ToFileData(TextureFileFormat format, byte? quality = null) {
        if (format == TextureFileFormat.Png && quality != null) {
            throw new ArgumentException("PNG does not support quality settings.", nameof(quality));
        }

        if (quality is > 100) {
            quality = 100;
        }

        CpuTextureData data = new CpuTextureData {
            Data = new InteropArray<byte>(this.data),
            Format = Format,
            ExtentX = Extent.X,
            ExtentY = Extent.Y,
            ExtentZ = 1
        };

        InteropResult<InteropArray<byte>> result = CpuTextureInterop.Encode(in data, format, quality);

        if (!result.TryGetValue(out InteropArray<byte> encoded, out ResultError error)) {
            error.ThrowAndDispose();
        }

        byte[] resultArray = encoded.AsSpan().ToArray();
        encoded.Dispose();
        return resultArray;
    }

}
