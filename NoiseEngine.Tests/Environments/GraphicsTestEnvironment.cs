using NoiseEngine.Tests.Fixtures;

namespace NoiseEngine.Tests.Environments;

[Collection(nameof(ApplicationCollection))]
public abstract class GraphicsTestEnvironment {

    protected ApplicationFixture Fixture { get; }

    protected GraphicsTestEnvironment(ApplicationFixture fixture) {
        Fixture = fixture;
    }

}
