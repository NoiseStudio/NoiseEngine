using NoiseEngine.Rendering;
using NoiseEngine.Tests.Fixtures;
using System.Collections.Generic;

namespace NoiseEngine.Tests.Rendering;

[Collection(nameof(ApplicationCollection))]
public class GraphicsTestEnvironment {

    protected ApplicationFixture Fixture { get; }
    protected IReadOnlyList<GraphicsDevice> Devices => Fixture.GraphicsDevices;

    public GraphicsTestEnvironment(ApplicationFixture fixture) {
        Fixture = fixture;
    }

}
