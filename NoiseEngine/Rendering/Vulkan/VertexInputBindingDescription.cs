using System.Runtime.InteropServices;

namespace NoiseEngine.Rendering.Vulkan;

/// <summary>
/// https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/VkVertexInputBindingDescription.html
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal readonly struct VertexInputBindingDescription {

    public uint Binding { get; }
    public uint Stride { get; }
    public VertexInputRate InputRate { get; }

    public VertexInputBindingDescription(uint binding, uint stride, VertexInputRate inputRate) {
        Binding = binding;
        Stride = stride;
        InputRate = inputRate;
    }

}
