using NoiseEngine.Mathematics;
using NoiseEngine.Nesl.Default;
using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.Emit.Attributes;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Environments.Nesl;
using NoiseEngine.Tests.Fixtures;
using System;

namespace NoiseEngine.Tests.Nesl.Operations.Branch;

public class CallTest : NeslTestEnvironment {

    public CallTest(ApplicationFixture fixture) : base(fixture) {
    }

    [Fact]
    public void CallWithArguments() {
        const float Value = 7.7f;

        BufferOutputTestHelper<float> helper = CreateBufferOutputTestHelper<float>();

        // Set method.
        NeslMethodBuilder set = helper.DefineMethod(null, BuiltInTypes.Float32);
        IlGenerator il = set.IlGenerator;

        il.Emit(OpCode.Load, 0u, 1u);
        il.Emit(OpCode.Return);

        // Main method.
        NeslMethodBuilder main = helper.DefineMethod();
        main.AddAttribute(KernelAttribute.Create(Vector3<uint>.One));
        il = main.IlGenerator;

        il.Emit(OpCode.DefVariable, BuiltInTypes.Float32);
        il.Emit(OpCode.LoadFloat32, 1u, Value);
        il.Emit(OpCode.Call, uint.MaxValue, set, stackalloc uint[] { 1u });
        il.Emit(OpCode.Return);

        // Assert.
        helper.ExecuteAndAssert(null, Value);
    }

    [Fact]
    public void CallWithoutArguments() {
        const float Value = 371.1667f;

        BufferOutputTestHelper<float> helper = CreateBufferOutputTestHelper<float>();

        // Set method.
        NeslMethodBuilder set = helper.DefineMethod();
        IlGenerator il = set.IlGenerator;

        il.Emit(OpCode.DefVariable, BuiltInTypes.Float32);
        il.Emit(OpCode.LoadFloat32, 1u, Value);
        il.Emit(OpCode.Load, 0u, 1u);
        il.Emit(OpCode.Return);

        // Main method.
        NeslMethodBuilder main = helper.DefineMethod();
        main.AddAttribute(KernelAttribute.Create(Vector3<uint>.One));
        il = main.IlGenerator;

        il.Emit(OpCode.Call, uint.MaxValue, set, Array.Empty<uint>());
        il.Emit(OpCode.Return);

        // Assert.
        helper.ExecuteAndAssert(null, Value);
    }

}