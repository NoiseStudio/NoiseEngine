using System;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Rendering.Vulkan;

internal class VulkanCameraDelegation : CameraDelegation {

    private RenderPass? renderPass;

    public new VulkanDevice GraphicsDevice => Unsafe.As<VulkanDevice>(base.GraphicsDevice);

    public RenderPass RenderPass {
        get {
            if (Camera.IsDirty)
                Calculate();
            return renderPass!;
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

        if (renderTarget is Texture) {
            renderPass = new TextureRenderPass(GraphicsDevice, renderTarget, Camera.ClearFlags);
            return;
        }

        throw new NotImplementedException("This camera's render target is not implemented.");
    }

}
