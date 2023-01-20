using NoiseEngine.Interop.Rendering.Presentation;
using NoiseEngine.Rendering;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;

namespace NoiseEngine.Tests;

public class CameraTest : ApplicationTestEnvironment {

    public CameraTest(ApplicationFixture fixture) : base(fixture) {
    }

    [FactRequire(TestRequirements.Gui)]
    public void DrawToWindow() {
        using Window window = new Window(nameof(CameraTest));

        ExecuteOnAllDevices(scene => {
            Camera camera = new Camera(scene) {
                RenderTarget = window,
                RenderLoop = new PerformanceRenderLoop() { FramesInFlight = 3 },
                ClearColor = Color.Random
            };

            while (!window.IsDisposed) {
                camera.ClearColor = Color.Random;
                WindowInterop.PoolEvents(window.Handle);
            }

            /*GraphicsCommandBuffer commandBuffer = new GraphicsCommandBuffer(scene.GraphicsDevice, false);
            while (!window.IsDisposed) {
                WindowInterop.PoolEvents(window.Handle);

                commandBuffer.AttachCameraUnchecked(camera);
                commandBuffer.DetachCameraUnchecked();

                commandBuffer.Execute();
                commandBuffer.Clear();
            }*/
        });
    }

}
