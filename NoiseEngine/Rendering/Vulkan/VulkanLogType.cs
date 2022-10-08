using System;

namespace NoiseEngine.Rendering.Vulkan;

/// <summary>
/// https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/VkDebugUtilsMessageTypeFlagBitsEXT.html
/// </summary>
[Flags]
internal enum VulkanLogType : uint {
    None = 0,
    General = 0x00000001,
    Validation = 0x00000002,
    Performance = 0x00000004,
    DeviceAddressBinding = 0x00000008,
    All = General | Validation | Performance | DeviceAddressBinding,
}
