using System.Runtime.InteropServices;

namespace NoiseEngine.Rendering.Vulkan;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct RenderPassCreateInfo(
    TextureFormat Format,
    uint SampleCount,
    CameraClearFlags ClearFlags
);
