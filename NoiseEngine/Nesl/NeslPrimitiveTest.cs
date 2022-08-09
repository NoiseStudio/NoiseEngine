using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;
using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;
using NoiseEngine.Nesl.Default;
using NoiseEngine.Nesl.Emit;
using NoiseEngine.Primitives;
using NoiseEngine.Rendering;

namespace NoiseEngine.Nesl;

internal static class NeslPrimitiveTest {

    internal static Shader CreateShader(GraphicsDevice graphicsDevice, string vertexShader) {
        IlGenerator? il;
        NeslAssemblyBuilder assembly = NeslAssemblyBuilder.DefineAssembly(nameof(NeslPrimitiveTest));

        NeslTypeBuilder shader = assembly.DefineType("Shader");

        NeslMethodBuilder fragment = shader.DefineMethod("Fragment", Vectors.Vector4, Vectors.Vector3);
        il = fragment.IlGenerator;

        il.Emit(OpCode.Return);

        // Compile
        return new Shader(
            graphicsDevice,
            ShaderCompiler.CompileGlsl(vertexShader, ShaderStage.Vertex),
            SpirVCompiler.Compile(new NeslEntryPoint[] {
                new NeslEntryPoint(fragment, ExecutionModel.Fragment)
            }).GetCode(),
            "main",
            fragment.Guid.ToString(),
            VertexPosition3Color3.GetVertexDescription()
        );
    }

}
