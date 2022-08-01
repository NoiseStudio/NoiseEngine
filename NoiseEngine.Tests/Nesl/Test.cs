using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.CompilerTools.Attributes;

namespace NoiseEngine.Tests.Nesl;

public class Test {

    [Fact]
    public void TestMethod() {
        IlGenerator? il;
        NeslAssemblyBuilder assembly = NeslAssemblyBuilder.DefineAssembly();

        // Default
        NeslTypeBuilder float32 = assembly.DefineType("System.Float32");
        float32.AddCustomAttribute(new PlatformDependentTypeRepresentationNeslAttribute("System.Single", null));

        NeslMethodBuilder float32Add = float32.DefineMethod("+op_Add");
        float32Add.AddAttribute(new PlatformDependentMethodNeslAttribute(null, null));

        // Shader
        NeslTypeBuilder shader = assembly.DefineType("Shader");

        NeslMethodBuilder main = shader.DefineMethod("Main");
        il = main.IlGenerator;

        il.Emit(OpCode.LoadFloat32, 8f);
        il.Emit(OpCode.LoadFloat32, 4f);
        il.Emit(OpCode.Call, float32Add);
        il.Emit(OpCode.Return);
    }

}
