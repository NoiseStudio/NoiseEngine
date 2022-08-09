using NoiseEngine.Nesl;
using NoiseEngine.Nesl.CompilerTools.Architectures.Cil;
using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;
using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;
using NoiseEngine.Nesl.Default;
using NoiseEngine.Nesl.Emit;
using NoiseEngine.Rendering;
using System;
using System.IO;

namespace NoiseEngine.Tests.Nesl;

public class Test {

    [Fact]
    public void TestCil() {
        IlGenerator? il;
        NeslAssemblyBuilder assembly = NeslAssemblyBuilder.DefineAssembly(nameof(TestCil));

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

        // Compile and run.
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

        NeslMethodBuilder main = shader.DefineMethod("Fragment", Vectors.Vector4, Vectors.Vector3);
        il = main.IlGenerator;

        il.Emit(OpCode.Return);

        // Compile.
        File.WriteAllBytes($"{nameof(TestSpirV)}.spv", SpirVCompiler.Compile(new NeslEntryPoint[] {
            new NeslEntryPoint(main, ExecutionModel.Fragment)
        }).GetCode());
    }

    [Fact]
    public void TestGlsl() {
        const string InColor3GlslFrag = @"
            #version 450
            layout(location = 0) in vec3 inColor;
            layout(location = 0) out vec4 outColor;
            void main() {
                outColor = vec4(inColor, 1.0);
            }
        ";

        // Compile.
        File.WriteAllBytes($"{nameof(TestGlsl)}.spv",
            ShaderCompiler.CompileGlsl(InColor3GlslFrag, ShaderStage.Fragment));
    }

}
