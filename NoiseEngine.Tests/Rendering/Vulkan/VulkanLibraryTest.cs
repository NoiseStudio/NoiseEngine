using NoiseEngine.Rendering.Vulkan;
using NoiseEngine.Tests.Fixtures;

namespace NoiseEngine.Tests.Rendering.Vulkan;

[Collection(nameof(ApplicationCollection))]
public class VulkanLibraryTest {

    [FactRequire(TestRequirements.Vulkan)]
    public void Create() {
        new VulkanLibrary();
    }

    [FactRequire(TestRequirements.Vulkan)]
    public void GetExtensionProperties() {
        _ = new VulkanLibrary().ExtensionProperties;
    }

    [FactRequire(TestRequirements.Vulkan)]
    public void GetLayerProperties() {
        _ = new VulkanLibrary().LayerProperties;
    }

}
