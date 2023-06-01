using System;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Cpu;

namespace NoiseEngine.Interop.Rendering;

internal static partial class CpuTextureInterop {

    [InteropImport("rendering_cpu_texture_interop_decode")]
    public static partial InteropResult<CpuTextureData> Decode(
        ReadOnlySpan<byte> fileData,
        TextureFileFormat fileFormat,
        InteropOption<TextureFormat> format
    );

}
