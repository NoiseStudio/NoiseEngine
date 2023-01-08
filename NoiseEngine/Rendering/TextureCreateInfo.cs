using NoiseEngine.Mathematics;
using NoiseEngine.Rendering.Vulkan;

namespace NoiseEngine.Rendering;

internal readonly record struct TextureCreateInfo(
    Vector3<uint> Size,
    TextureFormat Format,
    uint MipLevels,
    uint ArrayLayers,
    uint SampleCount,
    bool Linear,
    TextureUsage Usage,
    bool Concurrent,
    VulkanImageCreateInfo Vulkan
);
