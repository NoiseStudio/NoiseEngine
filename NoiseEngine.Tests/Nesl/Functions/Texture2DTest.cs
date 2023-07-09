using NoiseEngine.Components;
using NoiseEngine.DeveloperTools.Systems;
using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using NoiseEngine.Nesl;
using NoiseEngine.Primitives;
using NoiseEngine.Rendering;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace NoiseEngine.Tests.Nesl.Functions;

public class Texture2DTest : ApplicationTestEnvironment {

    [StructLayout(LayoutKind.Sequential)]
    readonly record struct VertexData(Vector3<float> Position, Vector2<float> Uv);

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

            Texture2D texture = Texture2D.FromFile(
                File.ReadAllBytes("C:\\Users\\Vixen\\Downloads\\cholibka.jpg"),
                scene.GraphicsDevice, TextureUsage.TransferAll | TextureUsage.ColorAttachment,
                TextureFormat.R8G8B8A8_SRGB
            );
            material.GetProperty("texture")!.SetTexture(texture);

            Mesh mesh = new Mesh<VertexData, ushort>(
                scene.GraphicsDevice,
                new VertexData[] {
                    // Top
                    new VertexData(new Vector3<float>(-0.5f, 0.5f, 0.5f), new Vector2<float>(0, 0)),
                    new VertexData(new Vector3<float>(0.5f, 0.5f, 0.5f), new Vector2<float>(1, 0)),
                    new VertexData(new Vector3<float>(-0.5f, 0.5f, -0.5f), new Vector2<float>(0, 1)),
                    new VertexData(new Vector3<float>(0.5f, 0.5f, -0.5f), new Vector2<float>(1, 1)),

                    // Bottom
                    new VertexData(new Vector3<float>(-0.5f, -0.5f, -0.5f), new Vector2<float>(0, 0)),
                    new VertexData(new Vector3<float>(0.5f, -0.5f, -0.5f), new Vector2<float>(1, 0)),
                    new VertexData(new Vector3<float>(-0.5f, -0.5f, 0.5f), new Vector2<float>(0, 1)),
                    new VertexData(new Vector3<float>(0.5f, -0.5f, 0.5f), new Vector2<float>(1, 1)),

                    // Right
                    new VertexData(new Vector3<float>(0.5f, -0.5f, -0.5f), new Vector2<float>(0, 0)),
                    new VertexData(new Vector3<float>(0.5f, 0.5f, -0.5f), new Vector2<float>(1, 0)),
                    new VertexData(new Vector3<float>(0.5f, -0.5f, 0.5f), new Vector2<float>(0, 1)),
                    new VertexData(new Vector3<float>(0.5f, 0.5f, 0.5f), new Vector2<float>(1, 1)),

                    // Left
                    new VertexData(new Vector3<float>(-0.5f, -0.5f, 0.5f), new Vector2<float>(0, 0)),
                    new VertexData(new Vector3<float>(-0.5f, 0.5f, 0.5f), new Vector2<float>(1, 0)),
                    new VertexData(new Vector3<float>(-0.5f, -0.5f, -0.5f), new Vector2<float>(0, 1)),
                    new VertexData(new Vector3<float>(-0.5f, 0.5f, -0.5f), new Vector2<float>(1, 1)),

                    // Front
                    new VertexData(new Vector3<float>(0.5f, -0.5f, 0.5f), new Vector2<float>(0, 0)),
                    new VertexData(new Vector3<float>(0.5f, 0.5f, 0.5f), new Vector2<float>(1, 0)),
                    new VertexData(new Vector3<float>(-0.5f, -0.5f, 0.5f), new Vector2<float>(0, 1)),
                    new VertexData(new Vector3<float>(-0.5f, 0.5f, 0.5f), new Vector2<float>(1, 1)),

                    // Back
                    new VertexData(new Vector3<float>(-0.5f, -0.5f, -0.5f), new Vector2<float>(0, 0)),
                    new VertexData(new Vector3<float>(-0.5f, 0.5f, -0.5f), new Vector2<float>(1, 0)),
                    new VertexData(new Vector3<float>(0.5f, -0.5f, -0.5f), new Vector2<float>(0, 1)),
                    new VertexData(new Vector3<float>(0.5f, 0.5f, -0.5f), new Vector2<float>(1, 1))
                },
                new ushort[] {
                    0, 1, 2, 3, 2, 1,
                    4, 5, 6, 7, 6, 5,
                    8, 9, 10, 11, 10, 9,
                    12, 13, 14, 15, 14, 13,
                    16, 17, 18, 19, 18, 17,
                    20, 21, 22, 23, 22, 21
                }
            );

            for (int x = -10; x < 10; x += 2) {
                for (int y = -10; y < 10; y += 2) {
                    scene.Spawn(new TransformComponent(new Vector3<float>(x, 0, y)), new MeshRendererComponent(
                        mesh, material
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
