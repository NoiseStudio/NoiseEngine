using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using System;

namespace NoiseEngine.Tests;

public class CameraClearFlagsTest : ApplicationTestEnvironment {

    public CameraClearFlagsTest(ApplicationFixture fixture) : base(fixture) {
    }

    [Fact]
    public void SolidColor() {
        Span<Color32> buffer = stackalloc Color32[1];

        foreach (GraphicsDevice device in GraphicsDevices) {
            Texture2D texture = new Texture2D(
                device, TextureUsage.TransferSource | TextureUsage.ColorAttachment, 1, 1
            );
            SimpleCamera camera = new SimpleCamera(device) {
                RenderTarget = new RenderTexture(texture),
                ClearFlags = CameraClearFlags.SolidColor,
                ClearColor = Color.Red,
                DepthTesting = false
            };

            GraphicsCommandBuffer commandBuffer = new GraphicsCommandBuffer(device, false);
            commandBuffer.AttachCameraUnchecked(camera);
            commandBuffer.DetachCameraUnchecked();

            commandBuffer.Execute();
            commandBuffer.Clear();

            // Assert.
            texture.GetPixels(buffer);
            Assert.Equal((Color32)camera.ClearColor, buffer[0]);
        }
    }

}
