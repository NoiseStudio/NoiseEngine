namespace NoiseEngine.Rendering.Vulkan;

internal class WindowRenderPass : RenderPass {

    public Swapchain Swapchain { get; }

    public WindowRenderPass(
        VulkanDevice device, Swapchain swapchain, ICameraRenderTarget renderTarget, CameraClearFlags clearFlags
    ) : base(device, renderTarget, new RenderPassCreateInfo(
        swapchain.Format, renderTarget.SampleCount, clearFlags, VulkanImageLayout.PresentSourceKHR
    )) {
        Swapchain = swapchain;
    }

}
