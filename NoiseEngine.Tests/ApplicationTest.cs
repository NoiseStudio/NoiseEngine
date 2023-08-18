using NoiseEngine.DeveloperTools.Systems;
using NoiseEngine.Jobs;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using System.Threading;

namespace NoiseEngine.Tests;

public class ApplicationTest : ApplicationTestEnvironment {

    public ApplicationTest(ApplicationFixture fixture) : base(fixture) {
    }

    [FactRequire(TestRequirements.Graphics | TestRequirements.Gui)]
    public void SimpleScene() {
        ExecuteOnAllDevices(scene => {
            Window window = Fixture.GetWindow("A lot of X-Cuboids 3090 Ti.");
            Camera camera = new Camera(scene) {
                RenderTarget = window,
                RenderLoop = new PerformanceRenderLoop()
            };

            for (int x = -10; x < 10; x += 2) {
                for (int y = -10; y < 10; y += 2) {
                    scene.Primitive.CreateCube(new pos3(x, 0, y));
                }
            }

            SystemCommands commands = new SystemCommands();
            commands.GetEntity(camera.Entity).Insert(new ApplicationTestSimpleSceneManagerComponent());
            camera.Scene.ExecuteCommands(commands);
            camera.Scene.AddFrameDependentSystem(new ApplicationTestSimpleSceneManagerSystem(scene, window));

            Thread.Sleep(1000);

            if (scene.HasAnySystem<DebugMovementSystem>()) {
                AutoResetEvent autoResetEvent = new AutoResetEvent(false);
                window.Disposed += (_, _) => autoResetEvent.Set();
                if (!window.IsDisposed)
                    autoResetEvent.WaitOne();
            }
        });
    }

}
