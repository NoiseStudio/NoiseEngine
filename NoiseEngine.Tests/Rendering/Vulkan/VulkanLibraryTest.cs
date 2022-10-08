using NoiseEngine.Rendering.Vulkan;
using NoiseEngine.Tests.Fixtures;

namespace NoiseEngine.Tests.Rendering.Vulkan;

[Collection(nameof(ApplicationCollection))]
public class VulkanLibraryTest {

    [Fact]
    public void Create() {
        new VulkanLibrary();
    }

}
