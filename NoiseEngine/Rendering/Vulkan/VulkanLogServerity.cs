using System;

namespace NoiseEngine.Rendering.Vulkan;

/// <summary>
/// https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/VkDebugUtilsMessageSeverityFlagBitsEXT.html
/// </summary>
[Flags]
internal enum VulkanLogSeverity : uint {
    None = 0,
    Verbose = 0x00000001,
    Info = 0x00000010,
    Warning = 0x00000100,
    Error = 0x00001000,
    All = Verbose | Info | Warning | Error
}
