using NoiseEngine.Nesl.CompilerTools.Architectures.Cil;
using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;
using NoiseEngine.Nesl.Default;
using NoiseEngine.Nesl.Emit;
using System;
using System.IO;

namespace NoiseEngine.Tests.Nesl;

public class Test {

    [Fact]
    public void TestMethod() {
        IlGenerator? il;
        NeslAssemblyBuilder assembly = NeslAssemblyBuilder.DefineAssembly(nameof(TestMethod));

        // Default
        /*NeslTypeBuilder float32 = assembly.DefineType("System.Float32", TypeAttributes.Public);
        float32.AddCustomAttribute(new PlatformDependentTypeRepresentationAttribute("System.Single", null));

        NeslMethodBuilder getNumber = float32.DefineMethod(
            "GetNumber", MethodAttributes.Public | MethodAttributes.Static, float32);
        //float32Add.AddAttribute(new PlatformDependentMethodNeslAttribute(null, null));
        il = getNumber.IlGenerator;

        il.Emit(OpCode.LoadFloat32, 20f);
        il.Emit(OpCode.Return);*/

        // Shader
        NeslTypeBuilder shader = assembly.DefineType("Shader");

        NeslFieldBuilder buffer = shader.DefineField("buffer", Buffers.ReadWriteBuffer);

        NeslMethodBuilder main = shader.DefineMethod("Main", BuiltInTypes.Float32);
        il = main.IlGenerator;

        il.Emit(OpCode.LoadArg, 0);
        il.Emit(OpCode.LoadField, buffer);
        il.Emit(OpCode.LoadUInt32, 5u);
        il.Emit(OpCode.LoadFloat32, 18.64f);
        il.Emit(OpCode.SetElement, BuiltInTypes.Float32);

        il.Emit(OpCode.LoadFloat32, 12f);
        il.Emit(OpCode.Return);

        // Compile
        CilCompiler compiler = new CilCompiler(assembly);

        Type type = compiler.Compile().GetType(shader.FullName)!;
        System.Reflection.MethodInfo methodInfo =
            type.GetMethod(main.Name, (System.Reflection.BindingFlags)int.MaxValue)!;
        System.Reflection.FieldInfo fieldInfo = type.GetField("buffer")!;

        object obj = Activator.CreateInstance(type)!;
        fieldInfo.SetValue(obj, new float[16]);

        Assert.Equal(12, (float)methodInfo.Invoke(obj, null)!);

        float[] b = (float[])fieldInfo.GetValue(obj)!;
        Assert.Equal(18.64f, b[5]);
    }

    [Fact]
    public void TestSpirV() {
        IlGenerator? il;
        NeslAssemblyBuilder assembly = NeslAssemblyBuilder.DefineAssembly(nameof(TestSpirV));

        NeslTypeBuilder shader = assembly.DefineType("Shader");

        NeslMethodBuilder main = shader.DefineMethod("Main");
        il = main.IlGenerator;

        il.Emit(OpCode.Return);

        // Compile
        File.WriteAllBytes($"{nameof(TestSpirV)}.spv", new SpirVCompiler(assembly).Compile());
    }

}
