using NoiseEngine.Rendering.Vulkan;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;

namespace NoiseEngine.Tests.Rendering.Vulkan;

public class SwapchainTest : ApplicationTestEnvironment {

    public SwapchainTest(ApplicationFixture fixture) : base(fixture) {
    }

    [FactRequire(TestRequirements.Vulkan | TestRequirements.Gui)]
    public void CreateAndDestroy() {
        using Window window = new Window(nameof(SwapchainTest));

        foreach (VulkanDevice device in VulkanDevices)
            _ = new Swapchain(device, window, 1);
    }

}
