using System;

namespace NoiseEngine.Rendering.Utils;

public static class TextureFormatUtils {

    /// <summary>
    /// Returns the size of a texel in bytes for the given <paramref name="format"/>.
    /// </summary>
    /// <param name="format">Format of the texture.</param>
    /// <returns>Size of a single texel in bytes.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Invalid <paramref name="format"/> was provided.</exception>
    public static int TexelSize(TextureFormat format) {
        return format switch {
            TextureFormat.R8_UNORM => 1,
            TextureFormat.R8_SRGB => 1,
            TextureFormat.R8G8_UNORM => 2,
            TextureFormat.R8G8_SRGB => 2,
            TextureFormat.R8G8B8_UNORM => 3,
            TextureFormat.R8G8B8_SRGB => 3,
            TextureFormat.R8G8B8A8_UNORM => 4,
            TextureFormat.R8G8B8A8_SRGB => 4,
            TextureFormat.R16_UNORM => sizeof(ushort) * 1,
            TextureFormat.R16G16_UNORM => sizeof(ushort) * 2,
            TextureFormat.R16G16B16_UNORM => sizeof(ushort) * 3,
            TextureFormat.R16G16B16A16_UNORM => sizeof(ushort) * 4,
            TextureFormat.R32G32B32_SFLOAT => sizeof(float) * 3,
            TextureFormat.R32G32B32A32_SFLOAT => sizeof(float) * 4,
            TextureFormat.D32_SFloat => sizeof(float) * 1,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

}
