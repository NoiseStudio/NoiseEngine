using NoiseEngine.Nesl.Emit;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using NoiseEngine.Tests.Nesl;
using System.Threading;

namespace NoiseEngine.Tests.Rendering;

public class ShaderTest : ApplicationTestEnvironment {

    public ShaderTest(ApplicationFixture fixture) : base(fixture) {
    }

    [FactRequire(TestRequirements.Gui | TestRequirements.Graphics)]
    public void Triangle() {
        // Create shader.
        NeslTypeBuilder shaderClassData = TestEmitHelper.NewType();

        Window window = Fixture.GetWindow(nameof(Triangle));
        foreach (GraphicsDevice device in GraphicsDevices) {
            Shader shader = new Shader(device, shaderClassData);

            SimpleCamera camera = new SimpleCamera(device) {
                RenderTarget = window,
            };

            GraphicsCommandBuffer commandBuffer = new GraphicsCommandBuffer(device, false);
            commandBuffer.AttachCameraUnchecked(camera);
            commandBuffer.DetachCameraUnchecked();

            commandBuffer.DrawMeshUnchecked(new Mesh(), new Material(shader));

            commandBuffer.Execute();
            commandBuffer.Clear();

            Thread.Sleep(100000);
        }
    }

}
