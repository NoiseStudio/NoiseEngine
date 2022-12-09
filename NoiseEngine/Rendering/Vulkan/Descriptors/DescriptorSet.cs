using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering.Vulkan.Descriptors;
using System;

namespace NoiseEngine.Rendering.Vulkan.Descriptors;

internal class DescriptorSet {

    public DescriptorSetLayout Layout { get; }

    internal InteropHandle<DescriptorSet> Handle { get; }

    public DescriptorSet(DescriptorSetLayout layout) {
        if (!DescriptorSetInterop.Create(layout.Handle).TryGetValue(
            out InteropHandle<DescriptorSet> handle, out ResultError error
        )) {
            error.ThrowAndDispose();
        }

        Layout = layout;
        Handle = handle;
    }

    ~DescriptorSet() {
        if (Handle.IsNull)
            return;

        DescriptorSetInterop.Destroy(Handle);
    }

    public void Update(DescriptorUpdateTemplate template, ReadOnlySpan<byte> data) {
        DescriptorSetInterop.Update(Handle, template.Handle, data);
    }

}
