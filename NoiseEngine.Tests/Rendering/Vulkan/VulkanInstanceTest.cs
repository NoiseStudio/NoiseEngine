using NoiseEngine.Rendering.Vulkan;
using NoiseEngine.Tests.Fixtures;

namespace NoiseEngine.Tests.Rendering.Vulkan;

[Collection(nameof(ApplicationCollection))]
public class VulkanInstanceTest {

    [FactRequire(TestRequirements.Vulkan)]
    public void CreateAndDispose() {
        using VulkanInstance instance = new VulkanInstance(new VulkanLibrary());
    }

}
