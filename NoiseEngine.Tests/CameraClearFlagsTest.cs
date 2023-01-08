using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;

namespace NoiseEngine.Tests;

[Collection(nameof(ApplicationCollection))]
public class CameraClearFlagsTest : ApplicationTestEnvironment {

    public CameraClearFlagsTest(ApplicationFixture fixture) : base(fixture) {
    }

    [Fact]
    public void SolidColor() {
        ExecuteOnAllDevices(scene => {
            Texture2D texture = new Texture2D(scene.GraphicsDevice, 16, 16);
            Camera camera = new Camera(scene) {
                RenderTarget = texture
            };

            GraphicsCommandBuffer commandBuffer = new GraphicsCommandBuffer(scene.GraphicsDevice, false);
            commandBuffer.AttachCameraUnchecked(camera);
            commandBuffer.DetachCameraUnchecked();

            commandBuffer.Execute();
            commandBuffer.Clear();
        });
    }

}
