using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering.Vulkan;
using NoiseEngine.Rendering.Vulkan.Descriptors;
using System;
using System.Collections.Generic;

namespace NoiseEngine.Rendering.Vulkan;

internal class PipelineLayout {

    public IReadOnlyList<DescriptorSetLayout> Layouts { get; }

    internal InteropHandle<PipelineLayout> Handle { get; }

    public PipelineLayout(
        IReadOnlyList<DescriptorSetLayout> layouts, ReadOnlySpan<PushConstantRange> pushConstantRanges
    ) {
        Span<InteropHandle<DescriptorSetLayout>> layoutHandles =
            stackalloc InteropHandle<DescriptorSetLayout>[layouts.Count];
        for (int i = 0; i < layoutHandles.Length; i++)
            layoutHandles[i] = layouts[i].Handle;

        if (!PipelineLayoutInterop.Create(layoutHandles, pushConstantRanges).TryGetValue(
            out InteropHandle<PipelineLayout> handle, out ResultError error
        )) {
            error.ThrowAndDispose();
        }

        Layouts = layouts;
        Handle = handle;
    }

    ~PipelineLayout() {
        if (Handle.IsNull)
            return;

        PipelineLayoutInterop.Destroy(Handle);
    }

}
