using NoiseEngine.Interop.InteropMarshalling;
using NoiseEngine.Rendering.Vulkan;
using System.Runtime.InteropServices;

namespace NoiseEngine.Interop.Rendering.Vulkan;

[StructLayout(LayoutKind.Sequential)]
internal readonly ref struct GraphicsPipelineCreateInfoRaw {

    public InteropReadOnlySpan<VertexInputBindingDescription> VertexInputBindingDescription { get; init; }
    public InteropReadOnlySpan<VertexInputAttributeDescription> VertexInputAttributeDescription { get; init; }
    public PrimitiveTopology PrimitiveTopology { get; init; }

};
