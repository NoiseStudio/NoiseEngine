using System;
using System.Runtime.InteropServices;

namespace NoiseEngine.Interop.Rendering.Vulkan.Buffers;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct VulkanBufferCreateReturnValue(IntPtr Handle, IntPtr InnerHandle);
