using NoiseEngine.Components;
using NoiseEngine.Mathematics;
using NoiseEngine.Primitives;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using System;

namespace NoiseEngine.Tests;

public class SimpleCameraTest : ApplicationTestEnvironment {

    public SimpleCameraTest(ApplicationFixture fixture) : base(fixture) {
    }


    [FactRequire(TestRequirements.Graphics)]
    public void DepthTesting() {
        ReadOnlySpan<VertexPosition3Color3> verticesA = stackalloc VertexPosition3Color3[] {
            new VertexPosition3Color3(new Vector3<float>(-0.5f, -0.5f, -0.5f), Vector3<float>.Up),
            new VertexPosition3Color3(new Vector3<float>(-0.5f, 0.5f, -0.5f), Vector3<float>.Up),
            new VertexPosition3Color3(new Vector3<float>(0.5f, -0.5f, -0.5f), Vector3<float>.Up),
            new VertexPosition3Color3(new Vector3<float>(0.5f, 0.5f, -0.5f), Vector3<float>.Up)
        };

        ReadOnlySpan<VertexPosition3Color3> verticesB = stackalloc VertexPosition3Color3[] {
            new VertexPosition3Color3(new Vector3<float>(-0.5f, -0.5f, -0.5f), Vector3<float>.Front),
            new VertexPosition3Color3(new Vector3<float>(-0.5f, 0.5f, -0.5f), Vector3<float>.Front),
            new VertexPosition3Color3(new Vector3<float>(0.5f, -0.5f, -0.5f), Vector3<float>.Front),
            new VertexPosition3Color3(new Vector3<float>(0.5f, 0.5f, -0.5f), Vector3<float>.Front)
        };

        ReadOnlySpan<ushort> indices = stackalloc ushort[] {
            0, 1, 2, 3, 2, 1
        };

        ReadOnlySpan<(Color32 expectedColor, bool depthTesting)> tasks = stackalloc (Color32, bool)[] {
            (Color32.Green, true),
            (Color32.Blue, false)
        };

        Span<Color32> buffer = stackalloc Color32[16 * 16];

        foreach (GraphicsDevice device in Fixture.GraphicsDevices) {
            PrimitiveCreatorShared shared = PrimitiveCreatorShared.CreateOrGet(device);

            Texture2D texture = new Texture2D(
                device, TextureUsage.TransferAll | TextureUsage.ColorAttachment, 16, 16, TextureFormat.R8G8B8A8_UNORM
            );
            SimpleCamera camera = new SimpleCamera(device) {
                RenderTarget = new RenderTexture(texture),
                ClearFlags = CameraClearFlags.SolidColor,
                ClearColor = Color.Red,
                ProjectionType = ProjectionType.Orthographic,
                OrthographicSize = 0.5f
            };

            GraphicsCommandBuffer commandBuffer = new GraphicsCommandBuffer(device, false);
            foreach ((Color32 expectedColor, bool depthTesting) in tasks) {
                camera.DepthTesting = depthTesting;

                commandBuffer.AttachCameraUnchecked(camera);

                commandBuffer.DrawMeshUnchecked(
                    new Mesh<VertexPosition3Color3, ushort>(device, verticesA, indices), shared.DefaultMaterial,
                    new TransformComponent(new pos3(0, 0, 5)).Matrix
                );
                commandBuffer.DrawMeshUnchecked(
                    new Mesh<VertexPosition3Color3, ushort>(device, verticesB, indices), shared.DefaultMaterial,
                    new TransformComponent(new pos3(0, 0, 10)).Matrix
                );

                commandBuffer.DetachCameraUnchecked();

                commandBuffer.Execute();
                commandBuffer.Clear();

                // Assert.
                texture.GetPixels(buffer);
                Assert.Equal(expectedColor, buffer[128]);
            }
        }
    }

}
