using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering.Vulkan;
using System.Diagnostics;

namespace NoiseEngine.Rendering.Vulkan;

internal class VulkanImageView {

    public Texture Texture { get; }
    public InteropHandle<VulkanImageView> Handle { get; }
    public InteropHandle<VulkanImageView> InnerHandle { get; }

    public VulkanImageView(Texture texture, VulkanImageViewCreateInfo createInfo) {
        Texture = texture;
        Debug.Assert(createInfo.ImageHandle == texture.Handle);

        if (!VulkanImageViewInterop.Create(createInfo).TryGetValue(
            out VulkanImageViewCreateReturnValue result, out ResultError error
        )) {
            error.ThrowAndDispose();
        }

        Handle = result.Handle;
        InnerHandle = result.InnerHandle;
    }

    ~VulkanImageView() {
        if (Handle == InteropHandle<VulkanImageView>.Zero)
            return;

        VulkanImageViewInterop.Destroy(Handle);
    }

}
