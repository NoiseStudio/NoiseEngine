using System;
using NoiseEngine.Rendering.Cpu;

namespace NoiseEngine.Interop.Rendering;

internal static partial class CpuTextureInterop {

    [InteropImport("rendering_cpu_texture_interop_decode_png")]
    public static partial InteropResult<CpuTextureData> DecodePng(ReadOnlySpan<byte> fileData);

}
