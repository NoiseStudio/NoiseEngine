using System;
using NoiseEngine.Mathematics;

namespace NoiseEngine.Rendering;

public abstract class CpuTexture {

    private readonly byte[] data;

    public ReadOnlySpan<byte> Data => data;

    public TextureFormat Format { get; }

    internal abstract Vector3<uint> Extent { get; }

    private protected CpuTexture(byte[] data, TextureFormat format) {
        this.data = data;
        Format = format;
    }

}
