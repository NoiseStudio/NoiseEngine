using NoiseEngine.Interop.InteropMarshalling;
using System;
using System.Runtime.InteropServices;

namespace NoiseEngine.Interop.Rendering.Vulkan;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct VulkanApplicationInfo(
    InteropString ApplicationName,
    VulkanVersion ApplicationVersion,
    VulkanVersion EngineVersion
) : IDisposable {

    public void Dispose() {
        ApplicationName.Dispose();
    }

}
