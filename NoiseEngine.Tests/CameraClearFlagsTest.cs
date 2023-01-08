using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using System;

namespace NoiseEngine.Tests;

[Collection(nameof(ApplicationCollection))]
public class CameraClearFlagsTest : ApplicationTestEnvironment {

    public CameraClearFlagsTest(ApplicationFixture fixture) : base(fixture) {
    }

    [Fact]
    public void SolidColor() {
        ExecuteOnAllDevices(scene => {
            Texture2D texture = new Texture2D(scene.GraphicsDevice, 1, 1);
            Camera camera = new Camera(scene) {
                RenderTarget = texture,
                ClearFlags = CameraClearFlags.SolidColor,
                ClearColor = Color.Red
            };

            GraphicsCommandBuffer commandBuffer = new GraphicsCommandBuffer(scene.GraphicsDevice, false);
            commandBuffer.AttachCameraUnchecked(camera);
            commandBuffer.DetachCameraUnchecked();

            commandBuffer.Execute();
            commandBuffer.Clear();

            // Assert.
            Span<Color32> buffer = stackalloc Color32[1];
            texture.GetPixels(buffer);

            Assert.Equal((Color32)camera.ClearColor, buffer[0]);
        });
    }

}
