using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NoiseEngine.Rendering.Vulkan;

internal class VulkanCameraDelegation : CameraDelegation {

    private readonly object calculateLocker = new object();

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
        lock (calculateLocker) {
            if (!Camera.IsDirty)
                return;

            ICameraRenderTarget? renderTarget = Camera.RenderTarget;
            if (renderTarget is null) {
                renderPass = null;
                throw new NullReferenceException("Camera's render target is null.");
            }

            if (renderTarget is Window window) {
                RenderPass? oldRenderPass = renderPass;
                Swapchain swapchain;

                if (oldRenderPass is null || !ReferenceEquals(oldRenderPass.RenderTarget, renderTarget))
                    swapchain = new Swapchain(GraphicsDevice, window);
                else
                    swapchain = ((WindowRenderPass)oldRenderPass).Swapchain;

                renderPass = new WindowRenderPass(GraphicsDevice, swapchain, renderTarget, Camera.ClearFlags);
            } else if (renderTarget is Texture) {
                renderPass = new TextureRenderPass(GraphicsDevice, renderTarget, Camera.ClearFlags);
            } else {
                throw new NotImplementedException("Camera render target is not implemented.");
            }

            Camera.IsDirty = false;
        }
    }

}
