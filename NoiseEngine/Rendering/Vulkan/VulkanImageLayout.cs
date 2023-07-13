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
    ShaderReadOnlyOptimal = 5,
    TransferSourceOptimal = 6,
    TransferDestinationOptimal = 7,
    Preinitialized = 8,

    AttachmentOptimal = 1000314001,

    PresentSourceKHR = 1000001002
}
