using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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

    internal IntPtr ClearColor { get; }

    public VulkanCameraDelegation(Camera camera) : base(camera) {
        ClearColor = Marshal.AllocHGlobal(Marshal.SizeOf<Color>());
    }

    ~VulkanCameraDelegation() {
        Marshal.FreeHGlobal(ClearColor);
    }

    public override void ClearRenderTarget() {
        renderPass = null;
    }

    public override void UpdateClearColor() {
        Marshal.StructureToPtr(Camera.ClearColor, ClearColor, false);
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
