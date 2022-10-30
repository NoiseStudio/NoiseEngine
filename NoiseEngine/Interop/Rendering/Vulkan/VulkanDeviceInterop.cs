using NoiseEngine.Interop.Rendering.Buffers;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Buffers;
using System;

namespace NoiseEngine.Interop.Rendering.Vulkan;

internal static partial class VulkanDeviceInterop {

    [InteropImport("rendering_vulkan_device_interop_destroy")]
    public static partial void Destroy(InteropHandle<GraphicsDevice> device);

    /// <summary>
    /// Initializes <paramref name="device"/>.
    /// </summary>
    /// <remarks>This method must be synchronized by caller.</remarks>
    /// <param name="device">Handle of existing device.</param>
    /// <returns><see cref="InteropResult{None}"/> with potential error.</returns>
    [InteropImport("rendering_vulkan_device_interop_initialize")]
    public static partial InteropResult<None> Initialize(InteropHandle<GraphicsDevice> device);

    [InteropImport("rendering_vulkan_device_interop_create_command_buffer")]
    public static partial InteropResult<InteropHandle<GraphicsCommandBuffer>> CreateCommandBuffer(
        InteropHandle<GraphicsDevice> device, ReadOnlySpan<byte> data, GraphicsCommandBufferUsage usage
    );

}
