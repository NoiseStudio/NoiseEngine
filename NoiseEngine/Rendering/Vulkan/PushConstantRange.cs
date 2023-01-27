using System.Runtime.InteropServices;

namespace NoiseEngine.Rendering.Vulkan;

/// <summary>
/// https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/VkPushConstantRange.html
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal readonly record struct PushConstantRange(ShaderStageFlags StageFlags, uint Offset, uint Size);
