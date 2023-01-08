using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering.Vulkan;

namespace NoiseEngine.Rendering.Vulkan;

internal class RenderPass {

    internal InteropHandle<RenderPass> Handle { get; }

    public RenderPass(VulkanDevice device, RenderPassCreateInfo createInfo) {
        if (!RenderPassInterop.Create(device.Handle, createInfo).TryGetValue(
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

}
