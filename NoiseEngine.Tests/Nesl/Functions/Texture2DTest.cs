using NoiseEngine.Components;
using NoiseEngine.DeveloperTools.Systems;
using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using NoiseEngine.Nesl;
using NoiseEngine.Rendering;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using System.Threading;

namespace NoiseEngine.Tests.Nesl.Functions;

public class Texture2DTest : ApplicationTestEnvironment {

    public Texture2DTest(ApplicationFixture fixture) : base(fixture) {
    }

    [FactRequire(TestRequirements.Graphics | TestRequirements.Gui)]
    public void Sample() {
        string path = "Path";
        NeslAssembly assembly = NeslCompiler.Compile(nameof(Sample), "", new NeslFile[] { new NeslFile(path, @"
            using System;

            struct VertexData {
                f32v3 Position;
                f32v2 Uv;
            }

            struct FragmentData {
                f32v4 Position;
                f32v2 Uv;
            }

            uniform Texture2D<f32v4> texture;

            FragmentData Vertex(VertexData data) {
                return new FragmentData() {
                    Position = VertexUtils.ObjectToClipPos(data.Position),
                    Uv = data.Uv
                };
            }

            f32v4 Fragment(FragmentData data) {
                return texture[data.Uv];
            }
        ") });

        ExecuteOnAllDevices(scene => {
            Window window = Fixture.GetWindow("Sample textures");
            Camera camera = new Camera(scene) {
                RenderTarget = window,
                RenderLoop = new PerformanceRenderLoop()
            };

            Shader shader = new Shader(scene.GraphicsDevice, assembly.GetType(path)!);
            Material material = new Material(shader);

            for (int x = -10; x < 10; x += 2) {
                for (int y = -10; y < 10; y += 2) {
                    scene.Spawn(new TransformComponent(new Vector3<float>(x, 0, y)), new MeshRendererComponent(
                        scene.Primitive.CubeMesh, material
                    ));
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
