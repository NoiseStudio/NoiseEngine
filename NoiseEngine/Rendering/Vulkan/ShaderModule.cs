using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering.Vulkan;
using System;

namespace NoiseEngine.Rendering.Vulkan;

internal class ShaderModule {

    public VulkanDevice Device { get; }

    internal InteropHandle<ShaderModule> Handle { get; }

    public ShaderModule(VulkanDevice device, ReadOnlySpan<byte> code) {
        if (!ShaderModuleInterop.Create(device.Handle, code).TryGetValue(
            out InteropHandle<ShaderModule> handle, out ResultError error
        )) {
            error.ThrowAndDispose();
        }

        Device = device;
        Handle = handle;
    }

    ~ShaderModule() {
        if (Handle.IsNull)
            return;

        ShaderModuleInterop.Destroy(Handle);
    }

}
