using NoiseEngine.Rendering;
using System.Runtime.InteropServices;

namespace NoiseEngine.Interop.Rendering;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct TextureSamplerCreateReturnValue(
    InteropHandle<TextureSampler> Handle, InteropHandle<TextureSampler> InnerHandle
);

