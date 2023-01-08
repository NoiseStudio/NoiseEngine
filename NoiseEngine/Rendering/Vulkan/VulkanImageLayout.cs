namespace NoiseEngine.Rendering.Vulkan;

/// <summary>
/// https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/VkImageLayout.html
/// </summary>
internal enum VulkanImageLayout : uint {
    Undefined = 0,
    General = 1,
    ColorAttachmentOptimal = 2,
    DepthStencilAttachmentOptimal = 3,
    DepthStencilReadOnlyOptimal = 4,
    TransferSourceOptimal = 5,
    TransferDestinationOptimal = 6,
    Preinitialized = 7
}
