using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering.Vulkan;
using System;

namespace NoiseEngine.Rendering.Vulkan;

internal class Framebuffer {

    internal InteropHandle<Framebuffer> Handle { get; }

    public Framebuffer(
        RenderPass renderPass, uint width, uint height, uint layers, ReadOnlySpan<VulkanImageViewCreateInfo> attachments
    ) {
        if (!FramebufferInterop.Create(
            renderPass.Handle, 0, width, height, layers, attachments
        ).TryGetValue(out InteropHandle<Framebuffer> handle, out ResultError error)) {
            error.ThrowAndDispose();
        }

        Handle = handle;
    }

    ~Framebuffer() {
        if (Handle == InteropHandle<Framebuffer>.Zero)
            return;

        FramebufferInterop.Destroy(Handle);
    }

}
