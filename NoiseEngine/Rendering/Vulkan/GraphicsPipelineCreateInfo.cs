using System;

namespace NoiseEngine.Rendering.Vulkan;

internal readonly ref struct GraphicsPipelineCreateInfo {

    public ReadOnlySpan<VertexInputBindingDescription> VertexInputBindingDescription { get; init; }
    public ReadOnlySpan<VertexInputAttributeDescription> VertexInputAttributeDescription { get; init; }
    public PrimitiveTopology PrimitiveTopology { get; init; }

}
