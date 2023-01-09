using System;

namespace NoiseEngine.Rendering;

public static class TextureFormatExtensions {

    public static ulong GetTexelSize(this TextureFormat format) {
        return format switch {
            TextureFormat.R8G8B8A8_SRGB => 4,
            _ => throw new NotImplementedException()
        };
    }

}
