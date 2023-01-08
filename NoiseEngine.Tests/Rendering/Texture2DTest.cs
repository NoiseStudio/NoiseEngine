using NoiseEngine.Rendering;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;

namespace NoiseEngine.Tests.Rendering;

public class Texture2DTest : GraphicsTestEnvironment {

    public Texture2DTest(ApplicationFixture fixture) : base(fixture) {
    }

    [Fact]
    public void CreateDestroy() {
        foreach (GraphicsDevice device in Fixture.GraphicsDevices)
            _ = new Texture2D(device, 256, 256);
    }

}
