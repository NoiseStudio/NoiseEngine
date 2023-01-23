using NoiseEngine.Rendering;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using System;

namespace NoiseEngine.Tests;

public class CameraTest : ApplicationTestEnvironment {

    public CameraTest(ApplicationFixture fixture) : base(fixture) {
    }

    [FactRequire(TestRequirements.Graphics)]
    public void RenderToWindow() {
        Window window = Fixture.GetWindow(nameof(RenderToWindow));
        ExecuteOnAllDevices(scene => {
            new Camera(scene) {
                RenderTarget = window,
                ClearColor = Color.Random
            }.Render();
        });
    }

    [FactRequire(TestRequirements.Graphics)]
    public void RenderToTexture() {
        ExecuteOnAllDevices(scene => {
            Texture2D texture = new Texture2D(
                scene.GraphicsDevice, TextureUsage.TransferSource | TextureUsage.ColorAttachment, 1, 1
            );

            Camera camera = new Camera(scene) {
                RenderTarget = texture,
                ClearFlags = CameraClearFlags.SolidColor,
                ClearColor = Color.Blue
            };
            camera.Render();

            // Assert.
            Span<Color32> buffer = stackalloc Color32[1];
            texture.GetPixels(buffer);
            Assert.Equal((Color32)camera.ClearColor, buffer[0]);
        });
    }

}
