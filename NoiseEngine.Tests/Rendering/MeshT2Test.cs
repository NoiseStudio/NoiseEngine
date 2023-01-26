﻿using NoiseEngine.Interop.Rendering.Presentation;
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

    private readonly record struct VertexData(Vector4<float> Position, Color Color);

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
            Mesh mesh = new Mesh<VertexData, ushort>(device, new VertexData[] {
                new VertexData(new Vector4<float>(-.5f, -.5f, 0, 1), Color.Magenta),
                new VertexData(new Vector4<float>(.5f, -.5f, 0, 1), Color.Red),
                new VertexData(new Vector4<float>(-.5f, .5f, 0, 1), Color.Green),
                new VertexData(new Vector4<float>(.5f, .5f, 0, 1), Color.Blue)
            }, new ushort[] {
                0, 1, 2, 1, 3, 2
            });

            for (int i = 0; i < 3000; i++) {
                WindowInterop.PoolEvents(window.Handle);

                commandBuffer.AttachCameraUnchecked(camera);
                commandBuffer.DrawMeshUnchecked(mesh, new Material(shader));
                commandBuffer.DetachCameraUnchecked();

                commandBuffer.Execute();
                commandBuffer.Clear();
            }

            // Assert.
            //texture.GetPixels(buffer);
            //Assert.Equal(Color32.Red, buffer[0]);
            //Assert.Equal((Color32)camera.ClearColor, buffer[3]);
        }
    }

}