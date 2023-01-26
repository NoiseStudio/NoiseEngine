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

        NeslTypeBuilder shaderClassData = TestEmitHelper.NewType();

        NeslMethodBuilder vertex = shaderClassData.DefineMethod(
            "Vertex", vertexData, Vectors.GetVector4(BuiltInTypes.Float32)
        );
        IlGenerator il = vertex.IlGenerator;

        il.Emit(OpCode.DefVariable, vertexData);
        il.Emit(OpCode.SetField, 1u, 0u, 0u);
        il.Emit(OpCode.ReturnValue, 1u);

        NeslMethodBuilder fragment = shaderClassData.DefineMethod("Fragment", Vectors.GetVector4(BuiltInTypes.Float32));
        il = fragment.IlGenerator;

        il.Emit(OpCode.DefVariable, Vectors.GetVector4(BuiltInTypes.Float32));
        il.Emit(OpCode.DefVariable, BuiltInTypes.Float32);
        il.Emit(OpCode.LoadFloat32, 1u, 1f);
        il.Emit(OpCode.SetField, 0u, 0u, 1u);
        il.Emit(OpCode.SetField, 0u, 3u, 1u);
        il.Emit(OpCode.LoadFloat32, 1u, 0f);
        il.Emit(OpCode.SetField, 0u, 1u, 1u);
        il.Emit(OpCode.SetField, 0u, 2u, 1u);
        il.Emit(OpCode.ReturnValue, 0u);

        // Executing.
        Span<Color32> buffer = stackalloc Color32[4];

        foreach (GraphicsDevice device in GraphicsDevices) {
            Shader shader = new Shader(device, shaderClassData);

            Texture2D texture = new Texture2D(
                device, TextureUsage.TransferSource | TextureUsage.ColorAttachment, 2, 2
            );

            Window window = Fixture.GetWindow(nameof(MeshT2Test));
            SimpleCamera camera = new SimpleCamera(device) {
                RenderTarget = window,
                ClearFlags = CameraClearFlags.SolidColor,
                //ClearColor = Color.Green
            };

            GraphicsCommandBuffer commandBuffer = new GraphicsCommandBuffer(device, false);
            Mesh mesh = new Mesh<Vector4<float>, ushort>(device, new Vector4<float>[] {
                new Vector4<float>(0, -.5f, 0, 1),
                new Vector4<float>(.5f, .5f, 0, 1),
                new Vector4<float>(-.5f, .5f, 0, 1)
            }, Array.Empty<ushort>());

            while (true) {
                WindowInterop.PoolEvents(window.Handle);

                commandBuffer.AttachCameraUnchecked(camera);
                commandBuffer.DrawMeshUnchecked(mesh, new Material(shader));
                commandBuffer.DetachCameraUnchecked();

                commandBuffer.Execute();
                commandBuffer.Clear();
            }

            // Assert.
            texture.GetPixels(buffer);
            Assert.Equal(Color32.Red, buffer[0]);
            Assert.Equal((Color32)camera.ClearColor, buffer[3]);
        }
    }

}
