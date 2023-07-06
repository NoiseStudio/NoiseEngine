using System;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Cpu;

namespace NoiseEngine.Interop.Rendering;

internal static partial class CpuTextureInterop {

    [InteropImport("rendering_cpu_texture_interop_decode")]
    public static partial InteropResult<CpuTextureData> Decode(
        ReadOnlySpan<byte> fileData,
        InteropOption<TextureFormat> format
    );

    [InteropImport("rendering_cpu_texture_interop_encode")]
    public static partial InteropResult<InteropArray<byte>> Encode(
        ReadOnlySpan<byte> data,
        uint width,
        uint height,
        TextureFormat format,
        TextureFileFormat fileFormat,
        InteropOption<byte> quality
    );

}
