using System.Runtime.InteropServices;

namespace NoiseEngine.Rendering.Vulkan;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct ComponentMapping(
    ComponentSwizzle R,
    ComponentSwizzle G,
    ComponentSwizzle B,
    ComponentSwizzle A
);
