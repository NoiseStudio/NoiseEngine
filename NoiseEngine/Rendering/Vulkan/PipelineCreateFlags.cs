using System;

namespace NoiseEngine.Rendering.Vulkan;

/// <summary>
/// https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/VkPipelineCreateFlagBits.html
/// </summary>
[Flags]
internal enum PipelineCreateFlags : uint {
    None = 0,
}
