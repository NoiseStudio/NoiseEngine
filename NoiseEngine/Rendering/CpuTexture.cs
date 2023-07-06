using System;
using NoiseEngine.Mathematics;
using NoiseEngine.Rendering.Utils;

namespace NoiseEngine.Rendering;

public abstract class CpuTexture {

    public abstract Span<byte> Data { get; }
    internal abstract Vector3<uint> Extent { get; }
    
    public TextureFormat Format { get; private set; }

    private protected CpuTexture(TextureFormat format) {
        Format = format;
    }
    
    /// <summary>
    /// Allows to change <see cref="CpuTexture"/>'s format without recreating it.
    /// Only possible if current and new formats have the same texel size.
    /// </summary>
    /// <param name="newFormat">New format.</param>
    /// <exception cref="ArgumentException">Old and new format have different texel sizes.</exception>
    public void ChangeFormat(TextureFormat newFormat) {
        if (TextureFormatUtils.TexelSize(Format) != TextureFormatUtils.TexelSize(newFormat)) {
            throw new ArgumentException("Cannot change texture format to a different texel size.", nameof(newFormat));
        }
        
        Format = newFormat;
    }

}
