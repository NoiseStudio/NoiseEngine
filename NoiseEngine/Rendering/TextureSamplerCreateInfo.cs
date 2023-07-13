using System.Runtime.InteropServices;

namespace NoiseEngine.Rendering;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct TextureSamplerCreateInfo(float MaxAnisotropy);
