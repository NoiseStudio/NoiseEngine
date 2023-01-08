using NoiseEngine.Rendering.Vulkan;
using System;

namespace NoiseEngine.Interop.Rendering.Vulkan;

internal static partial class FramebufferInterop {

    [InteropImport("rendering_vulkan_framebuffer_create")]
    public static partial InteropResult<InteropHandle<Framebuffer>> Create(
        InteropHandle<RenderPass> renderPass, uint flags, uint width, uint height, uint layers,
        ReadOnlySpan<VulkanImageViewCreateInfo> attachments
    );

    [InteropImport("rendering_vulkan_framebuffer_destroy")]
    public static partial void Destroy(InteropHandle<Framebuffer> handle);

}
