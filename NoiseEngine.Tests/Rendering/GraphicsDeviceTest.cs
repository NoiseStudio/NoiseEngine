using NoiseEngine.Rendering;
using NoiseEngine.Tests.Fixtures;

namespace NoiseEngine.Tests.Rendering;

[Collection(nameof(ApplicationCollection))]
public class GraphicsDeviceTest {

    [FactRequire(TestRequirements.Graphics)]
    public void Properties() {
        Assert.NotEmpty(Application.GraphicsInstance.Devices);

        foreach (GraphicsDevice device in Application.GraphicsInstance.Devices)
            Assert.False(device.Handle.IsNull);
    }

    [FactRequire(TestRequirements.Graphics)]
    public void Initialize() {
        foreach (GraphicsDevice device in Application.GraphicsInstance.Devices)
            device.Initialize();
    }

}
