namespace NoiseEngine.Rendering.Vulkan;

internal readonly record struct VulkanImageCreateInfo(
    uint Flags,
    VulkanImageType Type,
    VulkanImageLayout Layout
);
