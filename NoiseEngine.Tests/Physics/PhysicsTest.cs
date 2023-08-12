using NoiseEngine.Components;
using NoiseEngine.DeveloperTools.Systems;
using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using NoiseEngine.Physics;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using NoiseEngine.Tests.Jobs;
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

        scene.Spawn(
            new TransformComponent(
                new Vector3<float>(0, -105, 0), Quaternion<float>.Identity, new Vector3<float>(200, 200, 200)
            ),
            new MeshRendererComponent(scene.Primitive.GetSphereMesh(), scene.Primitive.DefaultMaterial),
            new ColliderComponent(new SphereCollider())
        );

        for (int x = 0; x < 1; x += 2) {
            for (int y = 0; y < 40; y += 2) {
                for (int z = 0; z < 1; z += 2) {
                    scene.Spawn(
                        new TransformComponent(new Vector3<float>(x, y * 3 + 4.5f, z)),
                        new MeshRendererComponent(scene.Primitive.GetSphereMesh(), scene.Primitive.DefaultMaterial),
                        new RigidBodyComponent(),
                        new ColliderComponent(new SphereCollider())
                    );
                }
            }
        }

        DebugMovementSystem.InitializeTo(camera);
        while (!window.IsDisposed)
            Thread.Sleep(10);
    }

}
