using NoiseEngine.Nesl;
using NoiseEngine.Nesl.CompilerTools;
using NoiseEngine.Nesl.CompilerTools.Attributes;
using NoiseEngine.Nesl.Emit;
using System;

namespace NoiseEngine.Tests.Nesl;

public class Test {

    [Fact]
    public void TestMethod() {
        IlGenerator? il;
        NeslAssemblyBuilder assembly = NeslAssemblyBuilder.DefineAssembly(nameof(TestMethod));

        // Default
        NeslTypeBuilder float32 = assembly.DefineType("System.Float32", TypeAttributes.Public);
        float32.AddCustomAttribute(new PlatformDependentTypeRepresentationNeslAttribute("System.Single", null));

        NeslMethodBuilder getNumber = float32.DefineMethod(
            "GetNumber", MethodAttributes.Public | MethodAttributes.Static, float32);
        //float32Add.AddAttribute(new PlatformDependentMethodNeslAttribute(null, null));
        il = getNumber.IlGenerator;

        il.Emit(OpCode.LoadFloat32, 20f);
        il.Emit(OpCode.Return);

        // Shader
        NeslTypeBuilder shader = assembly.DefineType("Shader");

        NeslMethodBuilder main = shader.DefineMethod("Main", MethodAttributes.Public, float32);
        il = main.IlGenerator;

        il.Emit(OpCode.LoadFloat32, 8f);
        il.Emit(OpCode.Call, getNumber);
        il.Emit(OpCode.Add);
        il.Emit(OpCode.Return);

        // Compile
        CilCompiler compiler = new CilCompiler(assembly);

        Type type = compiler.Compile().GetType(shader.FullName)!;
        System.Reflection.MethodInfo methodInfo = type.GetMethod(main.Name)!;

        Assert.Equal(5, (float)methodInfo.Invoke(Activator.CreateInstance(type), null)!);
    }

}
