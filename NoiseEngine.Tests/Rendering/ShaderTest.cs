using NoiseEngine.Mathematics;
using NoiseEngine.Nesl.Default;
using NoiseEngine.Nesl.Emit;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using NoiseEngine.Tests.Nesl;
using System;
using System.Buffers.Binary;

namespace NoiseEngine.Tests.Rendering;

public class ShaderTest : ApplicationTestEnvironment {

    public ShaderTest(ApplicationFixture fixture) : base(fixture) {
    }

    [FactRequire(TestRequirements.Graphics)]
    public void Triangle() {
        // Create shader.
        NeslTypeBuilder vertexData = TestEmitHelper.NewType();
        vertexData.DefineField("Position", Vectors.GetVector4(BuiltInTypes.Float32));

        NeslTypeBuilder shaderClassData = TestEmitHelper.NewType();

        NeslFieldBuilder positions = shaderClassData.DefineField(
            "positions", NoiseEngine.Nesl.Default.Buffers.GetReadWriteBuffer(Vectors.GetVector4(BuiltInTypes.Float32))
        );
        byte[] data = new byte[4 * sizeof(float) * 3];

        int index = 0;
        foreach (Vector4<float> vector in new Vector4<float>[] {
            new Vector4<float>(-1.0f, -1.0f, 0.0f, 1.0f),
            new Vector4<float>(1.0f, -1.0f, 0.0f, 1.0f),
            new Vector4<float>(-1.0f, 1.0f, 0.0f, 1.0f)
        }) {
            BinaryPrimitives.WriteSingleLittleEndian(data.AsSpan(index), vector.X);
            index += sizeof(float);
            BinaryPrimitives.WriteSingleLittleEndian(data.AsSpan(index), vector.Y);
            index += sizeof(float);
            BinaryPrimitives.WriteSingleLittleEndian(data.AsSpan(index), vector.Z);
            index += sizeof(float);
            BinaryPrimitives.WriteSingleLittleEndian(data.AsSpan(index), vector.W);
            index += sizeof(float);
        }

        positions.SetDefaultData(data);

        NeslMethodBuilder vertex = shaderClassData.DefineMethod("Vertex", vertexData);
        IlGenerator il = vertex.IlGenerator;

        il.Emit(OpCode.DefVariable, vertexData);
        il.Emit(OpCode.DefVariable, Vectors.GetVector4(BuiltInTypes.Float32));
        il.Emit(OpCode.DefVariable, BuiltInTypes.Int32);
        il.Emit(OpCode.Call, 3u, Vertex.Index, stackalloc uint[0]);
        il.Emit(OpCode.LoadElement, 2u, 0u, 3u);
        il.Emit(OpCode.SetField, 1u, 0u, 2u);
        il.Emit(OpCode.ReturnValue, 1u);

        NeslMethodBuilder fragment = shaderClassData.DefineMethod("Fragment", Vectors.GetVector4(BuiltInTypes.Float32));
        il = fragment.IlGenerator;

        il.Emit(OpCode.DefVariable, Vectors.GetVector4(BuiltInTypes.Float32));
        il.Emit(OpCode.DefVariable, BuiltInTypes.Float32);
        il.Emit(OpCode.LoadFloat32, 2u, 1f);
        il.Emit(OpCode.SetField, 1u, 0u, 2u);
        il.Emit(OpCode.SetField, 1u, 3u, 2u);
        il.Emit(OpCode.LoadFloat32, 2u, 0f);
        il.Emit(OpCode.SetField, 1u, 1u, 2u);
        il.Emit(OpCode.SetField, 1u, 2u, 2u);
        il.Emit(OpCode.ReturnValue, 1u);

        // Executing.
        Span<Color32> buffer = stackalloc Color32[4];

        foreach (GraphicsDevice device in GraphicsDevices) {
            Shader shader = new Shader(device, shaderClassData);

            Texture2D texture = new Texture2D(
                device, TextureUsage.TransferSource | TextureUsage.ColorAttachment, 2, 2
            );
            SimpleCamera camera = new SimpleCamera(device) {
                RenderTarget = texture,
                ClearFlags = CameraClearFlags.SolidColor,
                ClearColor = Color.Green
            };

            GraphicsCommandBuffer commandBuffer = new GraphicsCommandBuffer(device, false);
            commandBuffer.AttachCameraUnchecked(camera);
            commandBuffer.DrawMeshUnchecked(
                new Mesh<Vector4<float>, ushort>(device, Array.Empty<Vector4<float>>(), Array.Empty<ushort>()),
                new Material(shader)
            );
            commandBuffer.DetachCameraUnchecked();

            commandBuffer.Execute();
            commandBuffer.Clear();

            // Assert.
            texture.GetPixels(buffer);
            Assert.Equal(Color32.Red, buffer[0]);
            Assert.Equal((Color32)camera.ClearColor, buffer[3]);
        }
    }

}
