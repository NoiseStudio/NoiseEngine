using NoiseEngine.Components;
using NoiseEngine.DeveloperTools.Systems;
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

        MeshCollider collider = new MeshCollider(MeshColliderData.UnsafeCreateFromConvexHulls(new ConvexHull[] {
            new ConvexHull(new float3[] {
                new float3(-.5f, -.5f, -.5f),
                new float3(.5f, -.5f, -.5f),
                new float3(-.5f, -.5f, .5f),
                new float3(.5f, -.5f, .5f),
                new float3(-.5f, .5f, -.5f),
                new float3(.5f, .5f, -.5f),
                new float3(-.5f, .5f, .5f),
                new float3(.5f, .5f, .5f)
            }, float3.Zero, float.Sqrt(3) * 0.5f)
        }));

        scene.Spawn(
            new TransformComponent(
                new pos3(-0.5f, -105f, 0), Quaternion<float>.Identity, new float3(1, 1, 1) * 200
            ),
            new MeshRendererComponent(scene.Primitive.CubeMesh, scene.Primitive.DefaultMaterial),
            new ColliderComponent(collider)
        );

        for (int x = 0; x < 1; x += 2) {
            for (int y = 0; y < 40; y += 2) {
                for (int z = 0; z < 1; z += 2) {
                    scene.Spawn(
                        new TransformComponent(new pos3(x, y + 4.5f, z)),
                        new MeshRendererComponent(scene.Primitive.CubeMesh, scene.Primitive.DefaultMaterial),
                        new RigidBodyComponent(),
                        new ColliderComponent(collider)
                    );
                }
            }
        }

        DebugMovementSystem.InitializeTo(camera);
        while (!window.IsDisposed)
            Thread.Sleep(10);
    }

}
