using NoiseEngine.Components;
using NoiseEngine.Mathematics;
using NoiseEngine.Nesl;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using System;
using System.Collections.Generic;

namespace NoiseEngine.Tests.Nesl;

public class NeslCompilerTest : ApplicationTestEnvironment {

    public NeslCompilerTest(ApplicationFixture fixture) : base(fixture) {
    }

    internal static void ExecuteVector3PositionVector3Color(
        IEnumerable<GraphicsDevice> graphicsDevices, NeslAssembly assembly, string typeName
    ) {
        Span<Color32> buffer = stackalloc Color32[16 * 16];

        ReadOnlySpan<(Vector3<float>, Vector3<float>)> vertices = stackalloc (Vector3<float>, Vector3<float>)[] {
            (new Vector3<float>(-0.5f, -0.5f, 0f), Vector3<float>.Right),
            (new Vector3<float>(-0.5f, 0.5f, 0f), Vector3<float>.Right),
            (new Vector3<float>(-0.25f, -0.5f, 0f), Vector3<float>.Right),
            (new Vector3<float>(-0.25f, 0.5f, 0f), Vector3<float>.Right),
            (new Vector3<float>(0f, -0.5f, 0f), new Vector3<float>(1, 0, 1)),
            (new Vector3<float>(0f, 0.5f, 0f), new Vector3<float>(1, 0, 1)),
            (new Vector3<float>(0.25f, -0.5f, 0f), new Vector3<float>(1, 0, 1)),
            (new Vector3<float>(0.25f, 0.5f, 0f), new Vector3<float>(1, 0, 1))
        };
        ReadOnlySpan<ushort> triangles = stackalloc ushort[] {
            0, 1, 2, 1, 3, 2, 4, 5, 6, 5, 7, 6
        };

        foreach (GraphicsDevice device in graphicsDevices) {
            Shader shader = new Shader(device, assembly.GetType(typeName)!);

            Texture2D texture = new Texture2D(
                device, TextureUsage.TransferSource | TextureUsage.ColorAttachment, 16, 16, TextureFormat.R8G8B8A8_UNORM
            );
            SimpleCamera camera = new SimpleCamera(device) {
                RenderTarget = new RenderTexture(texture),
                ClearFlags = CameraClearFlags.SolidColor,
                ClearColor = Color.Green,
                DepthTesting = false,
                ProjectionType = ProjectionType.Orthographic,
                OrthographicSize = 0.5f
            };

            GraphicsCommandBuffer commandBuffer = new GraphicsCommandBuffer(device, false);
            commandBuffer.AttachCameraUnchecked(camera);
            commandBuffer.DrawMeshUnchecked(
                new Mesh<(Vector3<float>, Vector3<float>), ushort>(device, vertices, triangles), new Material(shader),
                new TransformComponent(new Vector3<float>(0, 0, 5)).Matrix
            );
            commandBuffer.DetachCameraUnchecked();

            commandBuffer.Execute();
            commandBuffer.Clear();

            // Assert.
            texture.GetPixels(buffer);

            for (int i = 0; i < buffer.Length; i += (int)texture.Width) {
                Assert.Equal(Color32.Red, buffer[i]);
                Assert.Equal(Color32.Green, buffer[i + 4]);
                Assert.Equal(Color32.Magenta, buffer[i + 8]);
                Assert.Equal(Color32.Green, buffer[i + 12]);
            }
        }
    }

    [Fact]
    public void Compile() {
        string path = "Path";
        NeslAssembly assembly = NeslCompiler.Compile(nameof(Compile), "", new NeslFile[] { new NeslFile(path, @"
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

        ExecuteVector3PositionVector3Color(GraphicsDevices, assembly, path);
    }

}
