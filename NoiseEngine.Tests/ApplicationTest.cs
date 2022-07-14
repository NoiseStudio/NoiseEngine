using NoiseEngine.DeveloperTools.Components;
using NoiseEngine.DeveloperTools.Systems;
using NoiseEngine.Mathematics;
using System.Threading;

namespace NoiseEngine.Tests;

public class ApplicationTest {

    [Fact]
    public void SimpleScene() {
        using Application application = Application.Create();
        ApplicationScene scene = new ApplicationScene(application);

        for (int x = -10; x < 10; x += 2) {
            for (int y = -10; y < 10; y += 2) {
                scene.Primitive.CreateCube(new Float3(x, 0, y));
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
