using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;
using NoiseEngine.Nesl.Emit;
using NoiseEngine.Primitives;
using NoiseEngine.Rendering;

namespace NoiseEngine.Nesl;

internal static class NeslPrimitiveTest {

    public static byte[] Code { get; }

    static NeslPrimitiveTest() {
        IlGenerator? il;
        NeslAssemblyBuilder assembly = NeslAssemblyBuilder.DefineAssembly(nameof(NeslPrimitiveTest));

        NeslTypeBuilder shader = assembly.DefineType("Shader");

        NeslMethodBuilder main = shader.DefineMethod("Main");
        il = main.IlGenerator;

        il.Emit(OpCode.Return);

        // Compile
        Code = null!;//new SpirVCompiler(assembly).Compile();
    }

    internal static Shader CreateShader(GraphicsDevice graphicsDevice, string vertexShader) {
        return new Shader(
            graphicsDevice,
            ShaderCompiler.CompileGlsl(vertexShader, ShaderStage.Vertex),
            Code,
            "main",
            "main",
            VertexPosition3Color3.GetVertexDescription()
        );
    }

}
