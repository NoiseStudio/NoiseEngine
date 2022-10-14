using NoiseEngine.Rendering;
using NoiseEngine.Tests.Fixtures;

namespace NoiseEngine.Tests.Rendering;

[Collection(nameof(ApplicationCollection))]
public class GraphicsPhysicalDeviceTest {

    [FactRequire(TestRequirements.Graphics)]
    public void Properties() {
        GraphicsPhysicalDevice physicalDevice = Application.GraphicsInstance.PhysicalDevices[0];

        Assert.False(physicalDevice.Handle.IsNull);
    }

}
