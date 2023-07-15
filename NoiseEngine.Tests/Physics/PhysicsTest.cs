using NoiseEngine.Components;
using NoiseEngine.DeveloperTools.Systems;
using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using NoiseEngine.Physics;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using System.Threading;

namespace NoiseEngine.Tests.Physics;

public class PhysicsTest : ApplicationTestEnvironment {

    public PhysicsTest(ApplicationFixture fixture) : base(fixture) {
    }

    [FactRequire(TestRequirements.Graphics | TestRequirements.Gui)]
    public void SimpleScene() {
        ApplicationScene scene = new ApplicationScene();
        Window window = Fixture.GetWindow("Physics!");
        Camera camera = new Camera(scene) {
            RenderTarget = window,
            RenderLoop = new PerformanceRenderLoop()
        };
        scene.AddFrameDependentSystem(new PhysicsTestActivatorSystem(scene, window));

        for (int x = 0; x < 1; x += 2) {
            for (int y = 0; y < 1; y += 2) {
                scene.Spawn(
                    new TransformComponent(new Vector3<float>(x, 0, y)),
                    new MeshRendererComponent(scene.Primitive.CubeMesh, scene.Primitive.DefaultMaterial),
                    new RigidBodyComponent()
                );
            }
        }

        DebugMovementSystem.InitializeTo(camera);
        Thread.Sleep(10000000);
    }

}
