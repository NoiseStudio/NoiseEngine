using NoiseEngine.DeveloperTools.Systems;
using NoiseEngine.Mathematics;
using System.Threading;
using NoiseEngine.Logging;

namespace NoiseEngine.Tests;

public class ApplicationTest {

    [FactRequire(TestRequirements.Gpu | TestRequirements.Gui)]
    public void SimpleScene() {
        Log.Logger.Sinks.Add(new ConsoleLogSink(new ConsoleLogSinkSettings { ThreadNameLength = 20 }));
        Log.Logger.Sinks.Add(FileLogSink.CreateFromDirectory("logs"));

        using Application application = Application.Create();
        ApplicationScene scene = new ApplicationScene(application);

        for (int x = -10; x < 10; x += 2) {
            for (int y = -10; y < 10; y += 2) {
                scene.Primitive.CreateCube(new Vector3<float>(x, 0, y));
            }
        }

        RenderCamera camera = scene.CreateWindow("A lot of X-Cuboids 3090 Ti.");
        camera.Entity.Add(scene.EntityWorld, new ApplicationTestSimpleSceneManagerComponent());
        camera.Scene.AddFrameDependentSystem(new ApplicationTestSimpleSceneManagerSystem(scene));

        Thread.Sleep(1000);

        if (scene.EntityWorld.HasAnySystem<DebugMovementSystem>())
            application.WaitToEnd();
    }

}
