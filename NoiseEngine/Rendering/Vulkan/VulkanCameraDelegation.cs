using System;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Rendering.Vulkan;

internal class VulkanCameraDelegation : CameraDelegation {

    private RenderPass? renderPass;

    public new VulkanDevice GraphicsDevice => Unsafe.As<VulkanDevice>(base.GraphicsDevice);

    public RenderPass? RenderPass {
        get {
            if (Camera.IsDirty)
                Calculate();
            return renderPass;
        }
    }

    public VulkanCameraDelegation(Camera camera) : base(camera) {
    }

    public override void ClearRenderTarget() {
        renderPass = null;
    }

    private void Calculate() {
        Camera.IsDirty = false;

        ICameraRenderTarget? renderTarget = Camera.RenderTarget;
        if (renderTarget is null) {
            renderPass = null;
            throw new NullReferenceException("Camera's render target is null.");
        }

        renderPass = new RenderPass(
            GraphicsDevice, new RenderPassCreateInfo(renderTarget.Format, renderTarget.SampleCount, Camera.ClearFlags)
        );
    }

}
