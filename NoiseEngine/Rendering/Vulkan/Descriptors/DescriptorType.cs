namespace NoiseEngine.Rendering.Vulkan.Descriptors;

/// <summary>
/// https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/VkDescriptorType.html
/// </summary>
internal enum DescriptorType : uint {
    CombinedImageSampler = 1,
    Uniform = 6,
    Storage = 7
}
