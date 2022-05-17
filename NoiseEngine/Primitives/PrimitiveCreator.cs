using NoiseEngine.Components;
using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using NoiseEngine.Rendering;
using System;
using System.Threading;

namespace NoiseEngine.Primitives {
    public class PrimitiveCreator : IDisposable {

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

        private readonly Application game;

        private Shader? defaultShader;
        private Material? defaultMaterial;

        private Mesh? cubeMesh;

        public Shader DefaultShader => defaultShader ??= Shader.FromGlslSource(
            game.GraphicsDevice,
            InPosition3Color3OutColor3GlslVert,
            InColor3GlslFrag,
            "main",
            "main",
            VertexPosition3Color3.GetVertexDescription());

        public Material DefaultMaterial => defaultMaterial ??= new Material(DefaultShader);

        internal PrimitiveCreator(Application game) {
            this.game = game;
        }

        /// <summary>
        /// Disposes this <see cref="PrimitiveCreator"/>.
        /// </summary>
        public void Dispose() {
            defaultMaterial?.Destroy();
            defaultShader?.Destroy();
        }

        /// <summary>
        /// Creates primitive cube.
        /// </summary>
        /// <param name="position">Position of the cube.</param>
        /// <param name="rotation">Rotation of the cube.</param>
        /// <param name="scale">Scale of the cube.</param>
        /// <returns>Cube <see cref="Entity"/>.</returns>
        public Entity CreateCube(Float3? position = null, Quaternion? rotation = null, Float3? scale = null) {
            if (cubeMesh == null) {
                Interlocked.CompareExchange(ref cubeMesh, new Mesh<VertexPosition3Color3, ushort>(
                    game.GraphicsDevice,
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
            }

            return game.World.NewEntity(
                new TransformComponent(position ?? Float3.Zero, rotation ?? Quaternion.Identity, scale ?? Float3.One),
                new MeshRendererComponent(cubeMesh!),
                new MaterialComponent(DefaultMaterial)
            );
        }

    }
}
