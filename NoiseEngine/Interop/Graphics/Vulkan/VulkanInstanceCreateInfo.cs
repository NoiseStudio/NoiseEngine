using NoiseEngine.Interop.InteropMarshalling;
using System;
using System.Runtime.InteropServices;

namespace NoiseEngine.Interop.Graphics.Vulkan;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct VulkanInstanceCreateInfo(
    InteropString ApplicationName,
    VulkanVersion ApplicationVersion,
    VulkanVersion EngineVersion
) : IDisposable {

    public void Dispose() {
        ApplicationName.Dispose();
    }

}
