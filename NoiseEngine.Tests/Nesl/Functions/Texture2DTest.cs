using NoiseEngine.Components;
using NoiseEngine.Mathematics;
using NoiseEngine.Nesl;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using System.IO;
using System.Runtime.InteropServices;

namespace NoiseEngine.Tests.Nesl.Functions;

public class Texture2DTest : ApplicationTestEnvironment {

    public Texture2DTest(ApplicationFixture fixture) : base(fixture) {
    }

    [Fact]
    public void Sample() {
        const int TextureSize = 64;
        const string Path = "Path";

        NeslAssembly assembly = NeslCompiler.Compile(nameof(Sample), "", new NeslFile[] { new NeslFile(Path, @"
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

        Color[] bufferA = new Color[TextureSize * TextureSize];
        Color[] bufferB = new Color[TextureSize * TextureSize];

        foreach (GraphicsDevice device in GraphicsDevices) {
            Texture2D result = new Texture2D(
                device, TextureUsage.TransferAll | TextureUsage.ColorAttachment,
                TextureSize, TextureSize, TextureFormat.R8G8B8A8_UNORM
            );
            SimpleCamera camera = new SimpleCamera(device) {
                RenderTarget = new RenderTexture(result),
                ClearFlags = CameraClearFlags.SolidColor,
                ClearColor = Color.Green,
                DepthTesting = false,
                ProjectionType = ProjectionType.Orthographic,
                OrthographicSize = 0.5f
            };

            Shader shader = new Shader(device, assembly.GetType(Path)!);
            Material material = new Material(shader);

            Texture2D texture = Texture2D.FromFile(
                File.ReadAllBytes("./Resources/Textures/Dummy64.webp"),
                device, TextureUsage.TransferAll | TextureUsage.Sampled, TextureFormat.R8G8B8A8_UNORM
            );
            material.GetProperty("texture")!.SetTexture(texture);

            GraphicsCommandBuffer commandBuffer = new GraphicsCommandBuffer(device, false);
            commandBuffer.AttachCameraUnchecked(camera);
            commandBuffer.DrawMeshUnchecked(new Mesh<VertexPosition3Uv2, ushort>(
                device,
                new VertexPosition3Uv2[] {
                    new VertexPosition3Uv2(new Vector3<float>(-0.5f, -0.5f, 0f), new Vector2<float>(0, 1)),
                    new VertexPosition3Uv2(new Vector3<float>(-0.5f, 0.5f, 0f), new Vector2<float>(0, 0)),
                    new VertexPosition3Uv2(new Vector3<float>(0.5f, -0.5f, 0f), new Vector2<float>(1, 1)),
                    new VertexPosition3Uv2(new Vector3<float>(0.5f, 0.5f, 0f), new Vector2<float>(1, 0))
                },
                new ushort[] {
                    0, 1, 2, 3, 2, 1
                }
            ), material, new TransformComponent(new pos3(0, 0, 5)).Matrix);
            commandBuffer.DetachCameraUnchecked();

            commandBuffer.Execute();
            commandBuffer.Clear();

            // Assert.
            texture.GetPixels<Color>(bufferA);
            result.GetPixels<Color>(bufferB);
            Assert.Equal(bufferA, bufferB);
        }
    }

}
