using NoiseEngine.Interop;
using NoiseEngine.Rendering;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using System.Threading.Tasks;

namespace NoiseEngine.Tests.Rendering;

public partial class GraphicsDeviceTest : GraphicsTestEnvironment {

    public GraphicsDeviceTest(ApplicationFixture fixture) : base(fixture) {
    }

    [InteropImport("rendering_vulkan_device_test_get_queue")]
    private static partial InteropResult<None> InteropUnmanagedGetQueue(InteropHandle<GraphicsDevice> device);

    [FactRequire(TestRequirements.Graphics)]
    public void Properties() {
        Assert.NotEmpty(Fixture.GraphicsDevices);

        foreach (GraphicsDevice device in Fixture.GraphicsDevices)
            Assert.False(device.Handle.IsNull);
    }

    [FactRequire(TestRequirements.Graphics)]
    public void Initialize() {
        foreach (GraphicsDevice device in Fixture.GraphicsDevices)
            device.Initialize();
    }

    [FactRequire(TestRequirements.Graphics)]
    public void UnmanagedGetQueue() {
        foreach (GraphicsDevice device in Fixture.GraphicsDevices) {
            device.Initialize();
            Parallel.For(0, 64, (_, _) => _ = InteropUnmanagedGetQueue(device.Handle).Value);
        }
    }

}
