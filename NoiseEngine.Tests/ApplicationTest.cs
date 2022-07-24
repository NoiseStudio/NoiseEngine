using NoiseEngine.DeveloperTools.Systems;
using NoiseEngine.Mathematics;
using System.Threading;

namespace NoiseEngine.Tests;

public class ApplicationTest {

    [FactRequire(TestRequirements.Gpu | TestRequirements.Gui)]
    public void SimpleScene() {
        Application.Initialize(new ApplicationSettings {
            ProcessExitOnApplicationExit = false
        });

        ApplicationScene scene = new ApplicationScene();

        for (int x = -10; x < 10; x += 2) {
            for (int y = -10; y < 10; y += 2) {
                scene.Primitive.CreateCube(new Vector3<float>(x, 0, y));
            }
        }

        RenderCamera camera = scene.CreateWindow("A lot of X-Cuboids 3090 Ti.");
        camera.Entity.Add(scene.EntityWorld, new ApplicationTestSimpleSceneManagerComponent());
        camera.Scene.AddFrameDependentSystem(new ApplicationTestSimpleSceneManagerSystem(scene));

        Thread.Sleep(1000);

        if (scene.EntityWorld.HasAnySystem<DebugMovementSystem>()) {
            AutoResetEvent autoResetEvent = new AutoResetEvent(false);
            Application.ApplicationExit += _ => autoResetEvent.Set();
            autoResetEvent.WaitOne();
        } else {
            Application.Exit();
        }
    }

}
