using NoiseEngine.Interop.Rendering.Presentation;
using NoiseEngine.Mathematics;
using NoiseEngine.Rendering;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using System.Threading;

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
                RenderLoop = new PerformanceRenderLoop(),
                ClearColor = Color.Random
            };

            //uint x = 10;
            while (!window.IsDisposed) {
                //camera.ClearColor = Color.Random;
                WindowInterop.PoolEvents(window.Handle);

                /*if (x < 1920) {
                    WindowInterop.SetPosition(window.Handle, null, new Vector2<uint>(x++, 720));
                    Thread.Sleep(1);
                }*/
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
