using NoiseEngine.Interop.Rendering.Presentation;
using NoiseEngine.Mathematics;
using NoiseEngine.Nesl.Default;
using NoiseEngine.Nesl.Emit;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using NoiseEngine.Tests.Nesl;
using System;

namespace NoiseEngine.Tests.Rendering;

public class MeshT2Test : ApplicationTestEnvironment {

    public MeshT2Test(ApplicationFixture fixture) : base(fixture) {
    }

    [FactRequire(TestRequirements.Graphics)]
    public void Figure2D() {
        // Create shader.
        NeslTypeBuilder vertexData = TestEmitHelper.NewType();
        vertexData.DefineField("Position", Vectors.GetVector4(BuiltInTypes.Float32));
        vertexData.DefineField("Color", Vectors.GetVector4(BuiltInTypes.Float32));

        NeslTypeBuilder shaderClassData = TestEmitHelper.NewType();

        NeslMethodBuilder vertex = shaderClassData.DefineMethod(
            "Vertex", vertexData, vertexData
        );
        IlGenerator il = vertex.IlGenerator;

        il.Emit(OpCode.DefVariable, vertexData);
        il.Emit(OpCode.Load, 1u, 0u);
        il.Emit(OpCode.ReturnValue, 1u);

        NeslMethodBuilder fragment = shaderClassData.DefineMethod(
            "Fragment", Vectors.GetVector4(BuiltInTypes.Float32), vertexData
        );
        il = fragment.IlGenerator;

        il.Emit(OpCode.DefVariable, Vectors.GetVector4(BuiltInTypes.Float32));
        il.Emit(OpCode.LoadField, 1u, 0u, 1u);
        il.Emit(OpCode.ReturnValue, 1u);

        // Executing.
        Span<Color32> buffer = stackalloc Color32[16 * 16];

        ReadOnlySpan<(Vector4<float>, Color)> vertices = stackalloc (Vector4<float>, Color)[] {
            (new Vector4<float>(-1, -1, 0, 1), Color.Red),
            (new Vector4<float>(-.5f, -1, 0, 1), Color.Red),
            (new Vector4<float>(-1, 1, 0, 1), Color.Red),
            (new Vector4<float>(-.5f, 1, 0, 1), Color.Red),
            (new Vector4<float>(0, -1, 0, 1), Color.Blue),
            (new Vector4<float>(.5f, -1, 0, 1), Color.Blue),
            (new Vector4<float>(0, 1, 0, 1), Color.Blue),
            (new Vector4<float>(.5f, 1, 0, 1), Color.Blue)
        };
        ReadOnlySpan<ushort> triangles = stackalloc ushort[] {
            0, 1, 2, 1, 3, 2, 4, 5, 6, 5, 7, 6
        };

        foreach (GraphicsDevice device in GraphicsDevices) {
            Shader shader = new Shader(device, shaderClassData);

            Texture2D texture = new Texture2D(
                device, TextureUsage.TransferSource | TextureUsage.ColorAttachment, 16, 16
            );
            SimpleCamera camera = new SimpleCamera(device) {
                RenderTarget = texture,
                ClearFlags = CameraClearFlags.SolidColor,
                ClearColor = Color.Green
            };

            GraphicsCommandBuffer commandBuffer = new GraphicsCommandBuffer(device, false);
            commandBuffer.AttachCameraUnchecked(camera);
            commandBuffer.DrawMeshUnchecked(
                new Mesh<(Vector4<float>, Color), ushort>(device, vertices, triangles), new Material(shader)
            );
            commandBuffer.DetachCameraUnchecked();

            commandBuffer.Execute();
            commandBuffer.Clear();

            // Assert.
            texture.GetPixels(buffer);

            for (int i = 0; i < buffer.Length; i += (int)texture.Width) {
                Assert.Equal(Color32.Red, buffer[i]);
                Assert.Equal(Color32.Green, buffer[i + 4]);
                Assert.Equal(Color32.Blue, buffer[i + 8]);
                Assert.Equal(Color32.Green, buffer[i + 12]);
            }
        }
    }

}
