using NoiseEngine.Interop.InteropMarshalling;
using NoiseEngine.Mathematics;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Vulkan;
using System.Runtime.InteropServices;

namespace NoiseEngine.Interop.Rendering.Vulkan;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct VulkanImageCreateInfoRaw(
    uint Flags,
    VulkanImageType Type,
    Vector3<uint> Extent,
    TextureFormat Format,
    uint MipLevels,
    uint ArrayLayers,
    uint SampleCount,
    InteropBool Linear,
    TextureUsage Usage,
    InteropBool Concurrent,
    VulkanImageLayout Layout
) {

    public VulkanImageCreateInfoRaw(TextureCreateInfo createInfo) : this(
        createInfo.Vulkan.Flags, createInfo.Vulkan.Type, createInfo.Size, createInfo.Format, createInfo.MipLevels,
        createInfo.ArrayLayers, createInfo.SampleCount, createInfo.Linear, createInfo.Usage, createInfo.Concurrent,
        createInfo.Vulkan.Layout
    ) {
    }

}
