using NoiseEngine.Rendering.Vulkan;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal readonly record struct VertexInputDescription(
    VertexInputBindingDescription[] Bindings,
    VertexInputAttributeDescription[] Attributes
);
