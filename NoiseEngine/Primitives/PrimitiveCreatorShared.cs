using NoiseEngine.Mathematics;
using NoiseEngine.Rendering;
using System;
using System.Threading;

namespace NoiseEngine.Primitives;

internal class PrimitiveCreatorShared : IDisposable {

    private const string InPosition3Color3OutColor3GlslVert = @"
            #version 450
            layout(location = 0) in vec3 inPosition;
            layout(location = 1) in vec3 inColor;
            layout(location = 0) out vec3 outColor;
            layout(binding = 0, set = 0) uniform FrameUniformData {
                vec4 Time; // Time since GraphicsDevice creation (t/20, t, t * 2, t * 3)
                vec4 SineTime; // Sine of time: (t/8, t/4, t/2, t)
                vec4 CosineTime; // Cosine of time: (t/8, t/4, t/2, t)
            } _FrameUniformData;
            layout(binding = 0, set = 1) uniform CameraUniformData {
                mat4 View;
                mat4 Projection;
                mat4 ViewProjection;
                vec4 DeltaTime; // (dt, 1/dt, 2*dt, 2/dt)
            } _CameraUniformData;
            layout(push_constant) uniform PushConstant {
                mat4 ModelMatrix;
            } _PushConstant;
            void main() {
                mat4 mvp = _CameraUniformData.ViewProjection * _PushConstant.ModelMatrix;
                gl_Position = mvp * vec4(inPosition, 1.0);
                outColor = inColor;
            }
        ";

    private const string InColor3GlslFrag = @"
            #version 450
            layout(location = 0) in vec3 inColor;
            layout(location = 0) out vec4 outColor;
            void main() {
                outColor = vec4(inColor, 1.0);
            }
        ";

    private readonly GraphicsDevice graphicsDevice;

    private Shader? defaultShader;
    private Material? defaultMaterial;

    private Mesh? cubeMesh;

    public Material DefaultMaterial => defaultMaterial ??= new Material(DefaultShader);
    public Shader DefaultShader => defaultShader ??= Shader.FromGlslSource(
        graphicsDevice,
        InPosition3Color3OutColor3GlslVert,
        InColor3GlslFrag,
        "main",
        "main",
        VertexPosition3Color3.GetVertexDescription());

    internal Mesh CubeMesh => cubeMesh ?? CreateCubeMesh();

    public PrimitiveCreatorShared(GraphicsDevice graphicsDevice) {
        this.graphicsDevice = graphicsDevice;
    }

    public void Dispose() {
        defaultMaterial?.Destroy();
        defaultShader?.Destroy();
    }

    private Mesh CreateCubeMesh() {
        Interlocked.CompareExchange(ref cubeMesh, new Mesh<VertexPosition3Color3, ushort>(
            graphicsDevice,
            new VertexPosition3Color3[] {
                // Top
                new VertexPosition3Color3(new Float3(-0.5f, 0.5f, 0.5f), Float3.Up),
                new VertexPosition3Color3(new Float3(0.5f, 0.5f, 0.5f), Float3.Up),
                new VertexPosition3Color3(new Float3(-0.5f, 0.5f, -0.5f), Float3.Up),
                new VertexPosition3Color3(new Float3(0.5f, 0.5f, -0.5f), Float3.Up),

                // Bottom
                new VertexPosition3Color3(new Float3(-0.5f, -0.5f, -0.5f), Float3.Up * 0.25f),
                new VertexPosition3Color3(new Float3(0.5f, -0.5f, -0.5f), Float3.Up * 0.25f),
                new VertexPosition3Color3(new Float3(-0.5f, -0.5f, 0.5f), Float3.Up * 0.25f),
                new VertexPosition3Color3(new Float3(0.5f, -0.5f, 0.5f), Float3.Up * 0.25f),

                // Right
                new VertexPosition3Color3(new Float3(0.5f, -0.5f, -0.5f), Float3.Right),
                new VertexPosition3Color3(new Float3(0.5f, 0.5f, -0.5f), Float3.Right),
                new VertexPosition3Color3(new Float3(0.5f, -0.5f, 0.5f), Float3.Right),
                new VertexPosition3Color3(new Float3(0.5f, 0.5f, 0.5f), Float3.Right),

                // Left
                new VertexPosition3Color3(new Float3(-0.5f, -0.5f, 0.5f), Float3.Right * 0.25f),
                new VertexPosition3Color3(new Float3(-0.5f, 0.5f, 0.5f), Float3.Right * 0.25f),
                new VertexPosition3Color3(new Float3(-0.5f, -0.5f, -0.5f), Float3.Right * 0.25f),
                new VertexPosition3Color3(new Float3(-0.5f, 0.5f, -0.5f), Float3.Right * 0.25f),

                // Front
                new VertexPosition3Color3(new Float3(0.5f, -0.5f, 0.5f), Float3.Front),
                new VertexPosition3Color3(new Float3(0.5f, 0.5f, 0.5f), Float3.Front),
                new VertexPosition3Color3(new Float3(-0.5f, -0.5f, 0.5f), Float3.Front),
                new VertexPosition3Color3(new Float3(-0.5f, 0.5f, 0.5f), Float3.Front),

                // Back
                new VertexPosition3Color3(new Float3(-0.5f, -0.5f, -0.5f), Float3.Front * 0.25f),
                new VertexPosition3Color3(new Float3(-0.5f, 0.5f, -0.5f), Float3.Front * 0.25f),
                new VertexPosition3Color3(new Float3(0.5f, -0.5f, -0.5f), Float3.Front * 0.25f),
                new VertexPosition3Color3(new Float3(0.5f, 0.5f, -0.5f), Float3.Front * 0.25f)
            },
            new ushort[] {
                0, 1, 2, 3, 2, 1,
                4, 5, 6, 7, 6, 5,
                8, 9, 10, 11, 10, 9,
                12, 13, 14, 15, 14, 13,
                16, 17, 18, 19, 18, 17,
                20, 21, 22, 23, 22, 21
            }
        ), null);

        return CubeMesh;
    }

}
