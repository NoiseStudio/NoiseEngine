using NoiseEngine.DeveloperTools.Components;
using NoiseEngine.DeveloperTools.Systems;
using NoiseEngine.Mathematics;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using System.Threading;

namespace NoiseEngine.Tests;

public class ApplicationTest : ApplicationTestEnvironment {

    public ApplicationTest(ApplicationFixture fixture) : base(fixture) {
    }


    [FactRequire(TestRequirements.Graphics | TestRequirements.Gui)]
    public void SimpleScene() {
        Window window = Fixture.GetWindow(nameof(SimpleScene));
        ExecuteOnAllDevices(scene => {
            Camera camera = new Camera(scene) {
                RenderTarget = window,
                RenderLoop = new PerformanceRenderLoop()
            };

            for (int x = -10; x < 10; x += 2) {
                for (int y = -10; y < 10; y += 2) {
                    scene.Primitive.CreateCube(new Vector3<float>(x, 0, y));
                }
            }

            camera.Entity.Add(scene.EntityWorld, new DebugMovementComponent());
            scene.AddFrameDependentSystem(new DebugMovementSystem(window));

            while (!window.IsDisposed)
                Thread.Sleep(1);
        });

        /*using ApplicationScene scene = new ApplicationScene();

        for (int x = -10; x < 10; x += 2) {
            for (int y = -10; y < 10; y += 2) {
                scene.Primitive.CreateCube(new Vector3<float>(x, 0, y));
            }
        }

        using RenderCamera camera = scene.CreateWindow("A lot of X-Cuboids 3090 Ti.");
        camera.Entity.Add(scene.EntityWorld, new ApplicationTestSimpleSceneManagerComponent());
        camera.Scene.AddFrameDependentSystem(new ApplicationTestSimpleSceneManagerSystem(scene));

        Thread.Sleep(1000);

        if (scene.EntityWorld.HasAnySystem<DebugMovementSystem>()) {
            AutoResetEvent autoResetEvent = new AutoResetEvent(false);
            Application.ApplicationExit += _ => autoResetEvent.Set();
            autoResetEvent.WaitOne();
        }*/
    }

}
