using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering.Vulkan.Descriptors;
using System;

namespace NoiseEngine.Rendering.Vulkan.Descriptors;

internal class DescriptorUpdateTemplate {

    public DescriptorSetLayout Layout { get; }

    internal InteropHandle<DescriptorUpdateTemplate> Handle { get; }

    public DescriptorUpdateTemplate(DescriptorSetLayout layout, ReadOnlySpan<DescriptorUpdateTemplateEntry> entries) {
        if (!DescriptorUpdateTemplateInterop.Create(layout.Handle, entries).TryGetValue(
            out InteropHandle<DescriptorUpdateTemplate> handle, out ResultError error
        )) {
            error.ThrowAndDispose();
        }

        Layout = layout;
        Handle = handle;
    }

    ~DescriptorUpdateTemplate() {
        if (Handle.IsNull)
            return;

        DescriptorUpdateTemplateInterop.Destroy(Handle);
    }

}
