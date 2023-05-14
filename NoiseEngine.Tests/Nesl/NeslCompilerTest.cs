using NoiseEngine.Components;
using NoiseEngine.DeveloperTools.Systems;
using NoiseEngine.Mathematics;
using NoiseEngine.Nesl;
using NoiseEngine.Rendering;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using System.Linq;
using System.Threading;

namespace NoiseEngine.Tests.Nesl;

public class NeslCompilerTest : ApplicationTestEnvironment {

    public NeslCompilerTest(ApplicationFixture fixture) : base(fixture) {
    }

    [Fact]
    public void Compile() {
        NeslAssembly assembly = NeslCompiler.Compile(nameof(Compile), new NeslFile[] { new NeslFile("Path", @"
            using System;

            struct VertexData {
                f32v3 Position;
                f32v3 Color;
            }

            struct FragmentData {
                f32v4 Position;
                f32v4 Color;
            }

            FragmentData Vertex(VertexData data) {
                return new FragmentData() {
                    Position = Vertex.ObjectToClipPos(data.Position),
                    Color = new f32v4(data.Color, data.Color.X)
                };
            }

            f32v4 Fragment(FragmentData data) {
                return data.Color;
            }
        ") });

        ExecuteOnAllDevices(scene => {
            Shader shader = new Shader(scene.GraphicsDevice, assembly.Types.OrderByDescending(x => x.Name.Length).First());

            Window window = Fixture.GetWindow("A lot of X-Cuboids 3090 Ti.");
            Camera camera = new Camera(scene) {
                RenderTarget = window,
                RenderLoop = new PerformanceRenderLoop()
            };

            for (int x = -10; x < 10; x += 2) {
                for (int y = -10; y < 10; y += 2) {
                    scene.EntityWorld.NewEntity(
                        new TransformComponent(new Vector3<float>(x, 0, y)),
                        new MeshRendererComponent(scene.Primitive.CubeMesh, new Material(shader))
                    );
                }
            }

            camera.Entity.Add(scene.EntityWorld, new ApplicationTestSimpleSceneManagerComponent());
            camera.Scene.AddFrameDependentSystem(new ApplicationTestSimpleSceneManagerSystem(scene, window));

            Thread.Sleep(1000);

            if (scene.EntityWorld.HasAnySystem<DebugMovementSystem>()) {
                AutoResetEvent autoResetEvent = new AutoResetEvent(false);
                window.Disposed += (_, _) => autoResetEvent.Set();
                if (!window.IsDisposed)
                    autoResetEvent.WaitOne();
            }
        });
    }

}
