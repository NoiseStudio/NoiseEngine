using System;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Rendering.Vulkan;

internal class RenderTextureRenderPass : RenderPass {

    public Framebuffer Framebuffer { get; }

    public new RenderTexture RenderTarget => Unsafe.As<RenderTexture>(base.RenderTarget);

    public RenderTextureRenderPass(
        VulkanDevice device, RenderTexture renderTarget, CameraClearFlags clearFlags, bool depthTesting
    ) : base(device, renderTarget, CreateRenderPassCreateInfo(renderTarget, clearFlags, depthTesting)) {
        Span<VulkanImageViewCreateInfo> attachments = stackalloc VulkanImageViewCreateInfo[depthTesting ? 2 : 1];

        attachments[0] = new VulkanImageViewCreateInfo(RenderTarget.Color.Handle, 0, VulkanImageViewType.Type2D,
            new ComponentMapping(
                ComponentSwizzle.Identity, ComponentSwizzle.Identity, ComponentSwizzle.Identity,
                ComponentSwizzle.Identity
            ),
        VulkanImageAspect.Color, 0, 1, 0, 1);

        if (depthTesting) {
            attachments[1] = new VulkanImageViewCreateInfo(
                RenderTarget.DepthStencil.Handle, 0, VulkanImageViewType.Type2D, new ComponentMapping(
                    ComponentSwizzle.Identity, ComponentSwizzle.Identity, ComponentSwizzle.Identity,
                    ComponentSwizzle.Identity
                ), VulkanImageAspect.Depth, 0, 1, 0, 1
            );
        }

        Framebuffer = new Framebuffer(this, renderTarget.Color.Extent.X, renderTarget.Color.Extent.Y, 1, attachments);
    }

    private static RenderPassCreateInfo CreateRenderPassCreateInfo(
        RenderTexture renderTarget, CameraClearFlags clearFlags, bool depthTesting
    ) {
        if (depthTesting) {
            return new RenderPassCreateInfo(
                renderTarget.Color.Format, renderTarget.Color.SampleCountInternal, clearFlags,
                VulkanImageLayout.TransferDestinationOptimal, true, renderTarget.DepthStencil.Format,
                renderTarget.DepthStencil.SampleCountInternal
            );
        }

        return new RenderPassCreateInfo(
            renderTarget.Color.Format, renderTarget.Color.SampleCountInternal, clearFlags,
            VulkanImageLayout.TransferDestinationOptimal, false, TextureFormat.R8G8B8A8_SRGB, 1
        );
    }

}
