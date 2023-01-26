using System.Runtime.InteropServices;

namespace NoiseEngine.Rendering.Vulkan;

/// <summary>
/// https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/VkVertexInputAttributeDescription.html
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal readonly record struct VertexInputAttributeDescription {

    public uint Location { get; }
    public uint Binding { get; }
    public VulkanFormat Format { get; }
    public uint Offset { get; }

    public VertexInputAttributeDescription(uint location, uint binding, VulkanFormat format, uint offset) {
        Location = location;
        Binding = binding;
        Format = format;
        Offset = offset;
    }

}
