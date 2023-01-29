namespace NoiseEngine.Rendering.Vulkan;

internal class WindowRenderPass : RenderPass {

    public Swapchain Swapchain { get; }

    public WindowRenderPass(
        VulkanDevice device, Swapchain swapchain, Window renderTarget, CameraClearFlags clearFlags, bool depthTesting
    ) : base(device, renderTarget, new RenderPassCreateInfo(
        swapchain.Format, 1, clearFlags, VulkanImageLayout.PresentSourceKHR,
        depthTesting, TextureFormat.D32_SFloat, 1
    )) {
        Swapchain = swapchain;
    }

}
