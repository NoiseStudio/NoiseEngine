using NoiseEngine.Interop;
using NoiseEngine.Rendering;
using NoiseEngine.Tests.Fixtures;
using System.Threading.Tasks;

namespace NoiseEngine.Tests.Rendering;

[Collection(nameof(ApplicationCollection))]
public partial class GraphicsDeviceTest {

    [InteropImport("graphics_vulkan_device_test_get_queue")]
    private static partial InteropResult<None> InteropUnmanagedGetQueue(InteropHandle<GraphicsDevice> device);

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

    [FactRequire(TestRequirements.Graphics)]
    public void UnmanagedGetQueue() {
        foreach (GraphicsDevice device in Application.GraphicsInstance.Devices) {
            device.Initialize();
            Parallel.For(0, 64, (_, _) => _ = InteropUnmanagedGetQueue(device.Handle).Value);
        }
    }

}
