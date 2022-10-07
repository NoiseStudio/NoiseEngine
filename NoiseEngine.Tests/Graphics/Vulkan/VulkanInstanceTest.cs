using NoiseEngine.Rendering.Vulkan;
using NoiseEngine.Tests.Fixtures;

namespace NoiseEngine.Tests.Graphics.Vulkan;

[Collection(nameof(ApplicationCollection))]
public class VulkanInstanceTest {

    [Fact]
    public void CreateAndDispose() {
        using VulkanInstance instance = new VulkanInstance();
    }

}
