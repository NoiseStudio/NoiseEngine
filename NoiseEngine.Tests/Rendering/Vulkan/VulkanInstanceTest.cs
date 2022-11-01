using NoiseEngine.Rendering.Vulkan;
using NoiseEngine.Tests.Fixtures;

namespace NoiseEngine.Tests.Rendering.Vulkan;

[Collection(nameof(ApplicationCollection))]
public class VulkanInstanceTest {

    [FactRequire(TestRequirements.Vulkan)]
    public void CreateAndDispose() {
        VulkanLibrary library = new VulkanLibrary();
        _ = new VulkanInstance(library, VulkanLogSeverity.All, VulkanLogType.All, library.SupportsValidationLayers);
    }

}
