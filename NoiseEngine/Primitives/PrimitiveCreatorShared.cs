using NoiseEngine.Mathematics;
using NoiseEngine.Nesl;
using NoiseEngine.Nesl.Default;
using NoiseEngine.Nesl.Emit;
using NoiseEngine.Primitives.Sphere;
using NoiseEngine.Rendering;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace NoiseEngine.Primitives;

internal class PrimitiveCreatorShared {

    private static readonly ConcurrentDictionary<GraphicsDevice, PrimitiveCreatorShared> shareds =
        new ConcurrentDictionary<GraphicsDevice, PrimitiveCreatorShared>();
    private static readonly NeslType defaultShaderClassData;

    private readonly Dictionary<(uint, SphereType), Mesh> sphereMeshes = new Dictionary<(uint, SphereType), Mesh>();

    private Shader? defaultShader;
    private Material? defaultMaterial;
    private Mesh? cubeMesh;

    public GraphicsDevice GraphicsDevice { get; }
    public Shader DefaultShader => defaultShader ?? CreateDefaultShader();
    public Material DefaultMaterial => defaultMaterial ?? CreateDefaultMaterial();
    public Mesh CubeMesh => cubeMesh ?? CreateCubeMesh();

    static PrimitiveCreatorShared() {
        NeslAssemblyBuilder assembly = NeslAssemblyBuilder.DefineAssembly("NoiseEngine");

        NeslTypeBuilder vertexData = assembly.DefineType(nameof(VertexPosition3Color3));
        vertexData.DefineField("Position", Vectors.GetVector3(BuiltInTypes.Float32));
        vertexData.DefineField("Color", Vectors.GetVector3(BuiltInTypes.Float32));

        NeslTypeBuilder fragmentData = assembly.DefineType("VertexPosition4Color4");
        fragmentData.DefineField("Position", Vectors.GetVector4(BuiltInTypes.Float32));
        fragmentData.DefineField("Color", Vectors.GetVector4(BuiltInTypes.Float32));

        NeslTypeBuilder defaultShaderClassData = assembly.DefineType("DefaultShader");

        NeslMethodBuilder vertex = defaultShaderClassData.DefineMethod(
            "Vertex", fragmentData, vertexData
        );
        vertex.SetModifiers(NeslModifiers.Static);
        IlGenerator il = vertex.IlGenerator;

        il.Emit(OpCode.DefVariable, fragmentData);
        il.Emit(OpCode.DefVariable, Vectors.GetVector4(BuiltInTypes.Float32));
        il.Emit(OpCode.DefVariable, Vectors.GetVector3(BuiltInTypes.Float32));
        il.Emit(OpCode.DefVariable, BuiltInTypes.Float32);

        il.Emit(OpCode.LoadField, 3u, 0u, 0u);
        il.Emit(OpCode.Call, 2u, VertexUtils.ObjectToClipPos, stackalloc uint[] { 3u });
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

        NeslMethodBuilder fragment = defaultShaderClassData.DefineMethod(
            "Fragment", Vectors.GetVector4(BuiltInTypes.Float32), fragmentData
        );
        fragment.SetModifiers(NeslModifiers.Static);
        il = fragment.IlGenerator;

        il.Emit(OpCode.DefVariable, Vectors.GetVector4(BuiltInTypes.Float32));
        il.Emit(OpCode.LoadField, 1u, 0u, 1u);
        il.Emit(OpCode.ReturnValue, 1u);

        PrimitiveCreatorShared.defaultShaderClassData = defaultShaderClassData;
    }

    private PrimitiveCreatorShared(GraphicsDevice graphicsDevice) {
        GraphicsDevice = graphicsDevice;
    }

    public static PrimitiveCreatorShared CreateOrGet(GraphicsDevice graphicsDevice) {
        return shareds.GetOrAdd(graphicsDevice, static (device) => new PrimitiveCreatorShared(device));
    }

    public Mesh GetSphereMesh(uint resolution, SphereType type) {
        if (sphereMeshes.TryGetValue((resolution, type), out Mesh? mesh))
            return mesh;

        lock (sphereMeshes) {
            if (sphereMeshes.TryGetValue((resolution, type), out mesh))
                return mesh;

            mesh = SpherePrimitveGenerator.GenerateMesh(this, resolution, type);
            sphereMeshes.Add((resolution, type), mesh);
            return mesh;
        }
    }

    private Shader CreateDefaultShader() {
        Interlocked.CompareExchange(ref defaultShader, new Shader(GraphicsDevice, defaultShaderClassData), null);
        return DefaultShader;
    }

    private Material CreateDefaultMaterial() {
        Interlocked.CompareExchange(ref defaultMaterial, new Material(DefaultShader), null);
        return DefaultMaterial;
    }

    private Mesh CreateCubeMesh() {
        Interlocked.CompareExchange(ref cubeMesh, new Mesh<VertexPosition3Color3, ushort>(
            GraphicsDevice,
            new VertexPosition3Color3[] {
                // Top
                new VertexPosition3Color3(new Vector3<float>(-0.5f, 0.5f, 0.5f), Vector3<float>.Up),
                new VertexPosition3Color3(new Vector3<float>(0.5f, 0.5f, 0.5f), Vector3<float>.Up),
                new VertexPosition3Color3(new Vector3<float>(-0.5f, 0.5f, -0.5f), Vector3<float>.Up),
                new VertexPosition3Color3(new Vector3<float>(0.5f, 0.5f, -0.5f), Vector3<float>.Up),

                // Bottom
                new VertexPosition3Color3(new Vector3<float>(-0.5f, -0.5f, -0.5f), Vector3<float>.Up * 0.25f),
                new VertexPosition3Color3(new Vector3<float>(0.5f, -0.5f, -0.5f), Vector3<float>.Up * 0.25f),
                new VertexPosition3Color3(new Vector3<float>(-0.5f, -0.5f, 0.5f), Vector3<float>.Up * 0.25f),
                new VertexPosition3Color3(new Vector3<float>(0.5f, -0.5f, 0.5f), Vector3<float>.Up * 0.25f),

                // Right
                new VertexPosition3Color3(new Vector3<float>(0.5f, -0.5f, -0.5f), Vector3<float>.Right),
                new VertexPosition3Color3(new Vector3<float>(0.5f, 0.5f, -0.5f), Vector3<float>.Right),
                new VertexPosition3Color3(new Vector3<float>(0.5f, -0.5f, 0.5f), Vector3<float>.Right),
                new VertexPosition3Color3(new Vector3<float>(0.5f, 0.5f, 0.5f), Vector3<float>.Right),

                // Left
                new VertexPosition3Color3(new Vector3<float>(-0.5f, -0.5f, 0.5f), Vector3<float>.Right * 0.25f),
                new VertexPosition3Color3(new Vector3<float>(-0.5f, 0.5f, 0.5f), Vector3<float>.Right * 0.25f),
                new VertexPosition3Color3(new Vector3<float>(-0.5f, -0.5f, -0.5f), Vector3<float>.Right * 0.25f),
                new VertexPosition3Color3(new Vector3<float>(-0.5f, 0.5f, -0.5f), Vector3<float>.Right * 0.25f),

                // Front
                new VertexPosition3Color3(new Vector3<float>(0.5f, -0.5f, 0.5f), Vector3<float>.Front),
                new VertexPosition3Color3(new Vector3<float>(0.5f, 0.5f, 0.5f), Vector3<float>.Front),
                new VertexPosition3Color3(new Vector3<float>(-0.5f, -0.5f, 0.5f), Vector3<float>.Front),
                new VertexPosition3Color3(new Vector3<float>(-0.5f, 0.5f, 0.5f), Vector3<float>.Front),

                // Back
                new VertexPosition3Color3(new Vector3<float>(-0.5f, -0.5f, -0.5f), Vector3<float>.Front * 0.25f),
                new VertexPosition3Color3(new Vector3<float>(-0.5f, 0.5f, -0.5f), Vector3<float>.Front * 0.25f),
                new VertexPosition3Color3(new Vector3<float>(0.5f, -0.5f, -0.5f), Vector3<float>.Front * 0.25f),
                new VertexPosition3Color3(new Vector3<float>(0.5f, 0.5f, -0.5f), Vector3<float>.Front * 0.25f)
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
