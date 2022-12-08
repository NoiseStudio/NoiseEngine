using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering.Vulkan.Descriptors;
using System;

namespace NoiseEngine.Rendering.Vulkan.Descriptors;

internal class DescriptorSetLayout {

    public VulkanDevice Device { get; }

    internal InteropHandle<DescriptorSetLayout> Handle { get; }

    public DescriptorSetLayout(VulkanDevice device, ReadOnlySpan<DescriptorSetLayoutBinding> bindings) {
        device.Initialize();

        if (!DescriptorSetLayoutInterop.Create(device.Handle, 0, bindings).TryGetValue(
            out InteropHandle<DescriptorSetLayout> handle, out ResultError error
        )) {
            error.ThrowAndDispose();
        }

        Device = device;
        Handle = handle;
    }

    ~DescriptorSetLayout() {
        if (Handle.IsNull)
            return;

        DescriptorSetLayoutInterop.Destroy(Handle);
    }

}
