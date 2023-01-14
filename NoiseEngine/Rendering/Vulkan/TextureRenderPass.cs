using System.Runtime.CompilerServices;

namespace NoiseEngine.Rendering.Vulkan;

internal class TextureRenderPass : RenderPass {

    public Framebuffer Framebuffer { get; }

    public new Texture RenderTarget => Unsafe.As<Texture>(base.RenderTarget);

    public TextureRenderPass(
        VulkanDevice device, ICameraRenderTarget renderTarget, CameraClearFlags clearFlags
    ) : base(device, renderTarget, new RenderPassCreateInfo(
        renderTarget.Format, renderTarget.SampleCount, clearFlags, VulkanImageLayout.TransferDestinationOptimal
    )) {
        Framebuffer = new Framebuffer(
            this, renderTarget.Extent.X, renderTarget.Extent.Y, 1, stackalloc VulkanImageViewCreateInfo[] {
                new VulkanImageViewCreateInfo(
                    RenderTarget.Handle, 0, VulkanImageViewType.Type2D, new ComponentMapping(
                        ComponentSwizzle.Identity, ComponentSwizzle.Identity, ComponentSwizzle.Identity,
                        ComponentSwizzle.Identity
                    ), VulkanImageAspect.Color, 0, 1, 0, 1
                )
        });
    }

}
