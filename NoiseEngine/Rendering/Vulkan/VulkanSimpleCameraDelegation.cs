using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NoiseEngine.Rendering.Vulkan;

internal class VulkanSimpleCameraDelegation : SimpleCameraDelegation {

    private readonly object calculateLocker = new object();

    private RenderPass? renderPass;
    private uint imageCount = 1;

    public new VulkanDevice GraphicsDevice => Unsafe.As<VulkanDevice>(base.GraphicsDevice);

    public RenderPass RenderPass {
        get {
            if (Camera.IsDirty)
                Calculate();
            return renderPass!;
        }
    }

    internal IntPtr ClearColor { get; }

    public VulkanSimpleCameraDelegation(SimpleCamera camera) : base(camera) {
        ClearColor = Marshal.AllocHGlobal(Marshal.SizeOf<Color>());
    }

    ~VulkanSimpleCameraDelegation() {
        Marshal.FreeHGlobal(ClearColor);
    }

    public override void ClearRenderTarget() {
        renderPass = null;
    }

    public override void UpdateClearColor() {
        Marshal.StructureToPtr(Camera.ClearColor, ClearColor, false);
    }

    public override uint ChangeFramesInFlightCount(uint targetFramesInFlightCount) {
        imageCount = targetFramesInFlightCount;
        if (!TryGetSwapchain(out Swapchain? swapchain))
            return 0;

        uint result = swapchain.ChangeMinImageCount(targetFramesInFlightCount);
        imageCount = result;
        return result;
    }

    private void Calculate() {
        lock (calculateLocker) {
            if (!Camera.IsDirty)
                return;

            ICameraRenderTarget? renderTarget = Camera.RenderTarget;
            if (renderTarget is null) {
                renderPass = null;
                Camera.IsDirty = false;
                throw new NullReferenceException("Camera's render target is null.");
            }

            if (renderTarget is Window window)
                CreateRenderPassWindow(window);
            else if (renderTarget is Texture)
                renderPass = new TextureRenderPass(GraphicsDevice, renderTarget, Camera.ClearFlags);
            else
                throw new NotImplementedException("Camera render target is not implemented.");

            Camera.IsDirty = false;
        }
    }

    private void CreateRenderPassWindow(Window window) {
        RenderPass? oldRenderPass = renderPass;
        Swapchain swapchain;

        if (oldRenderPass is null || !ReferenceEquals(oldRenderPass.RenderTarget, window))
            swapchain = new Swapchain(GraphicsDevice, window, imageCount);
        else
            swapchain = ((WindowRenderPass)oldRenderPass).Swapchain;

        renderPass = new WindowRenderPass(GraphicsDevice, swapchain, window, Camera.ClearFlags);
    }

    private bool TryGetSwapchain([NotNullWhen(true)] out Swapchain? swapchain) {
        RenderPass? renderPass = this.renderPass;
        if (renderPass is WindowRenderPass windowRenderPass) {
            swapchain = windowRenderPass.Swapchain;
            return true;
        }

        lock (calculateLocker) {
            ICameraRenderTarget? renderTarget = Camera.RenderTarget;
            if (renderTarget is Window window) {
                CreateRenderPassWindow(window);
                swapchain = ((WindowRenderPass)this.renderPass!).Swapchain;
                return true;
            }

            Camera.IsDirty = false;
            swapchain = null;
            return false;
        }
    }

}
