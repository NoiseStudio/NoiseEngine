using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;
using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;
using NoiseEngine.Nesl.Default;
using NoiseEngine.Nesl.Emit;
using NoiseEngine.Primitives;
using NoiseEngine.Rendering;
using System;

namespace NoiseEngine.Nesl;

internal static class NeslPrimitiveTest {

    public static byte[] Code { get; }
    public static Guid Guid { get; }

    static NeslPrimitiveTest() {
        IlGenerator? il;
        NeslAssemblyBuilder assembly = NeslAssemblyBuilder.DefineAssembly(nameof(NeslPrimitiveTest));

        NeslTypeBuilder shader = assembly.DefineType("Shader");

        NeslMethodBuilder main = shader.DefineMethod("Fragment", Vectors.Vector4, Vectors.Vector3);
        il = main.IlGenerator;

        il.Emit(OpCode.Return);

        // Compile
        Code = SpirVCompiler.Compile(new NeslEntryPoint[] {
            new NeslEntryPoint(main, ExecutionModel.Fragment)
        }).GetCode();

        Guid = main.Guid;
    }

    internal static Shader CreateShader(GraphicsDevice graphicsDevice, string vertexShader) {
        return new Shader(
            graphicsDevice,
            ShaderCompiler.CompileGlsl(vertexShader, ShaderStage.Vertex),
            Code,
            "main",
            Guid.ToString(),
            VertexPosition3Color3.GetVertexDescription()
        );
    }

}
