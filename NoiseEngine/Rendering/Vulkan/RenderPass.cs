using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering.Vulkan;

namespace NoiseEngine.Rendering.Vulkan;

internal abstract class RenderPass {

    public ICameraRenderTarget RenderTarget { get; }

    internal InteropHandle<RenderPass> Handle { get; }

    protected RenderPass(VulkanDevice device, ICameraRenderTarget renderTarget, CameraClearFlags clearFlags) {
        RenderTarget = renderTarget;

        if (!RenderPassInterop.Create(
            device.Handle, new RenderPassCreateInfo(renderTarget.Format, renderTarget.SampleCount, clearFlags)
        ).TryGetValue(
            out InteropHandle<RenderPass> handle, out ResultError error
        )) {
            error.ThrowAndDispose();
        }

        Handle = handle;
    }

    ~RenderPass() {
        if (Handle == InteropHandle<RenderPass>.Zero)
            return;

        RenderPassInterop.Destroy(Handle);
    }

    public abstract Framebuffer GetFramebuffer();

}
