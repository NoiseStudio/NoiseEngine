using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering;
using NoiseEngine.Mathematics;
using NoiseEngine.Rendering.Cpu;

namespace NoiseEngine.Rendering;

public class CpuTexture2D : CpuTexture {

    private readonly InteropArray<byte> data;

    public uint Width => Extent.X;
    public uint Height => Extent.Y;

    /// <summary>
    /// Data of the image in row-major order (index = x + Size.X * y).
    /// </summary>
    public override Span<byte> Data => data.AsSpan();

    internal override Vector3<uint> Extent { get; }

    private CpuTexture2D(InteropArray<byte> data, TextureFormat format, Vector2<uint> size) : base(format) {
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

        if (!result.TryGetValue(out CpuTextureData data, out _)) {
            texture = null;
            return false;
        }

        Debug.Assert(data.ExtentZ == 1);
        texture = new CpuTexture2D(data.Data, data.Format, new Vector2<uint>(data.ExtentX, data.ExtentY));
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
        if (!TryFromFile(fileData, out CpuTexture2D? texture, format))
            throw new ArgumentException("Cannot decode texture from given file data.", nameof(fileData));

        return texture;
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

}
