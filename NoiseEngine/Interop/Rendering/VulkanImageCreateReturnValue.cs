using NoiseEngine.Rendering;
using System.Runtime.InteropServices;

namespace NoiseEngine.Interop.Rendering;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct TextureCreateReturnValue(
    InteropHandle<Texture> Handle, InteropHandle<Texture> InnerHandle
);
