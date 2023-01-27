using NoiseEngine.Components;
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
                new Mesh<(Vector4<float>, Color), ushort>(device, vertices, triangles), new Material(shader),
                new Matrix4x4<float>()
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

    [FactRequire(TestRequirements.Graphics)]
    public void Figure3D() {
        // Create shader.
        NeslTypeBuilder vertexData = TestEmitHelper.NewType();
        vertexData.DefineField("Position", Vectors.GetVector3(BuiltInTypes.Float32));
        vertexData.DefineField("Color", Vectors.GetVector3(BuiltInTypes.Float32));

        NeslTypeBuilder fragmentData = TestEmitHelper.NewType();
        fragmentData.DefineField("Position", Vectors.GetVector4(BuiltInTypes.Float32));
        fragmentData.DefineField("Color", Vectors.GetVector4(BuiltInTypes.Float32));

        NeslTypeBuilder shaderClassData = TestEmitHelper.NewType();

        NeslMethodBuilder vertex = shaderClassData.DefineMethod(
            "Vertex", fragmentData, vertexData
        );
        IlGenerator il = vertex.IlGenerator;

        il.Emit(OpCode.DefVariable, fragmentData);
        il.Emit(OpCode.DefVariable, Vectors.GetVector4(BuiltInTypes.Float32));
        il.Emit(OpCode.DefVariable, Vectors.GetVector3(BuiltInTypes.Float32));
        il.Emit(OpCode.DefVariable, BuiltInTypes.Float32);

        il.Emit(OpCode.LoadField, 3u, 0u, 0u);
        il.Emit(OpCode.Call, 2u, Vertex.ObjectToClipPos, stackalloc uint[] { 3u });
        il.Emit(OpCode.SetField, 1u, 0u, 2u);

        il.Emit(OpCode.LoadField, 3u, 0u, 1u);

        il.Emit(OpCode.LoadField, 4u, 3u, 0u);
        il.Emit(OpCode.SetField, 2u, 0u, 4u);
        il.Emit(OpCode.LoadField, 4u, 3u, 1u);
        il.Emit(OpCode.SetField, 2u, 1u, 4u);
        il.Emit(OpCode.LoadField, 4u, 3u, 2u);
        il.Emit(OpCode.SetField, 2u, 2u, 4u);
        il.Emit(OpCode.LoadFloat32, 4u, 1f);
        il.Emit(OpCode.SetField, 2u, 3u, 4u);

        il.Emit(OpCode.SetField, 1u, 1u, 2u);

        il.Emit(OpCode.ReturnValue, 1u);

        NeslMethodBuilder fragment = shaderClassData.DefineMethod(
            "Fragment", Vectors.GetVector4(BuiltInTypes.Float32), fragmentData
        );
        il = fragment.IlGenerator;

        il.Emit(OpCode.DefVariable, Vectors.GetVector4(BuiltInTypes.Float32));
        il.Emit(OpCode.LoadField, 1u, 0u, 1u);
        il.Emit(OpCode.ReturnValue, 1u);

        // Executing.
        Span<Color32> buffer = stackalloc Color32[16 * 16];

        (Quaternion<float>, Color32)[] tasks = new (Quaternion<float>, Color32)[] {
            (Quaternion<float>.Identity, new Color32(0, 0, 64)),
            (Quaternion.EulerDegrees(new Vector3<float>(0, 90, 0)), new Color32(255, 0, 0)),
            (Quaternion.EulerDegrees(new Vector3<float>(0, 180, 0)), new Color32(0, 0, 255)),
            (Quaternion.EulerDegrees(new Vector3<float>(0, 270, 0)), new Color32(64, 0, 0)),
            (Quaternion.EulerDegrees(new Vector3<float>(90, 0, 0)), new Color32(0, 64, 0)),
            (Quaternion.EulerDegrees(new Vector3<float>(270, 0, 0)), new Color32(0, 255, 0)),
        };

        ReadOnlySpan<(Vector3<float>, Vector3<float>)> vertices = stackalloc (Vector3<float>, Vector3<float>)[] {
            // Top
            (new Vector3<float>(-0.5f, 0.5f, 0.5f), Vector3<float>.Up),
            (new Vector3<float>(0.5f, 0.5f, 0.5f), Vector3<float>.Up),
            (new Vector3<float>(-0.5f, 0.5f, -0.5f), Vector3<float>.Up),
            (new Vector3<float>(0.5f, 0.5f, -0.5f), Vector3<float>.Up),

            // Bottom
            (new Vector3<float>(-0.5f, -0.5f, -0.5f), Vector3<float>.Up * 0.25f),
            (new Vector3<float>(0.5f, -0.5f, -0.5f), Vector3<float>.Up * 0.25f),
            (new Vector3<float>(-0.5f, -0.5f, 0.5f), Vector3<float>.Up * 0.25f),
            (new Vector3<float>(0.5f, -0.5f, 0.5f), Vector3<float>.Up * 0.25f),

            // Right
            (new Vector3<float>(0.5f, -0.5f, -0.5f), Vector3<float>.Right),
            (new Vector3<float>(0.5f, 0.5f, -0.5f), Vector3<float>.Right),
            (new Vector3<float>(0.5f, -0.5f, 0.5f), Vector3<float>.Right),
            (new Vector3<float>(0.5f, 0.5f, 0.5f), Vector3<float>.Right),

            // Left
            (new Vector3<float>(-0.5f, -0.5f, 0.5f), Vector3<float>.Right * 0.25f),
            (new Vector3<float>(-0.5f, 0.5f, 0.5f), Vector3<float>.Right * 0.25f),
            (new Vector3<float>(-0.5f, -0.5f, -0.5f), Vector3<float>.Right * 0.25f),
            (new Vector3<float>(-0.5f, 0.5f, -0.5f), Vector3<float>.Right * 0.25f),

            // Front
            (new Vector3<float>(0.5f, -0.5f, 0.5f), Vector3<float>.Front),
            (new Vector3<float>(0.5f, 0.5f, 0.5f), Vector3<float>.Front),
            (new Vector3<float>(-0.5f, -0.5f, 0.5f), Vector3<float>.Front),
            (new Vector3<float>(-0.5f, 0.5f, 0.5f), Vector3<float>.Front),

            // Back
            (new Vector3<float>(-0.5f, -0.5f, -0.5f), Vector3<float>.Front * 0.25f),
            (new Vector3<float>(-0.5f, 0.5f, -0.5f), Vector3<float>.Front * 0.25f),
            (new Vector3<float>(0.5f, -0.5f, -0.5f), Vector3<float>.Front * 0.25f),
            (new Vector3<float>(0.5f, 0.5f, -0.5f), Vector3<float>.Front * 0.25f)
        };
        ReadOnlySpan<ushort> triangles = stackalloc ushort[] {
            0, 1, 2, 3, 2, 1,
            4, 5, 6, 7, 6, 5,
            8, 9, 10, 11, 10, 9,
            12, 13, 14, 15, 14, 13,
            16, 17, 18, 19, 18, 17,
            20, 21, 22, 23, 22, 21
        };

        foreach (GraphicsDevice device in GraphicsDevices) {
            Shader shader = new Shader(device, shaderClassData);

            Texture2D texture = new Texture2D(
                device, TextureUsage.TransferSource | TextureUsage.ColorAttachment, 16, 16, TextureFormat.R8G8B8A8_UNORM
            );
            SimpleCamera camera = new SimpleCamera(device) {
                RenderTarget = texture,
                ClearFlags = CameraClearFlags.SolidColor,
                ClearColor = Color.White,
                ProjectionType = ProjectionType.Orthographic,
                OrthographicSize = 0.5f
            };

            Mesh mesh = new Mesh<(Vector3<float>, Vector3<float>), ushort>(device, vertices, triangles);
            Material material = new Material(shader);
            TransformComponent transform = new TransformComponent(new Vector3<float>(0, 0, 5));

            GraphicsCommandBuffer commandBuffer = new GraphicsCommandBuffer(device, false);

            foreach ((Quaternion<float> rotation, Color32 expectedColor) in tasks) {
                commandBuffer.AttachCameraUnchecked(camera);
                commandBuffer.DrawMeshUnchecked(mesh, material, (transform with { Rotation = rotation }).Matrix);
                commandBuffer.DetachCameraUnchecked();

                commandBuffer.Execute();
                commandBuffer.Clear();

                // Assert.
                texture.GetPixels(buffer);
                Assert.Equal(expectedColor, buffer[128]);
            }

            commandBuffer.AttachCameraUnchecked(camera);
            commandBuffer.DrawMeshUnchecked(
                mesh, material, (transform with { Position = new Vector3<float>(0, 0, -100) }
            ).Matrix);
            commandBuffer.DetachCameraUnchecked();

            commandBuffer.Execute();
            commandBuffer.Clear();

            // Assert.
            texture.GetPixels(buffer);
            Assert.Equal((Color32)camera.ClearColor, buffer[128]);
        }
    }

}
