using NoiseEngine.Rendering.Vulkan;
using NoiseEngine.Rendering;
using NoiseEngine.Tests.Fixtures;
using System.Collections.Generic;

namespace NoiseEngine.Tests.Environments;

[Collection(nameof(ApplicationCollection))]
public abstract class GraphicsTestEnvironment {

    protected ApplicationFixture Fixture { get; }

    private protected IReadOnlyList<GraphicsDevice> GraphicsDevices => Fixture.GraphicsDevices;
    private protected IReadOnlyList<VulkanDevice> VulkanDevices => Fixture.VulkanDevices;

    protected GraphicsTestEnvironment(ApplicationFixture fixture) {
        Fixture = fixture;
    }

}
