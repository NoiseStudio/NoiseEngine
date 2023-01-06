using NoiseEngine.Mathematics;
using NoiseEngine.Nesl.Default;
using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.Emit.Attributes;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Environments.Nesl;
using NoiseEngine.Tests.Fixtures;

namespace NoiseEngine.Tests.Nesl.Intrinsics;

public class GlobalInvocationTest : NeslTestEnvironment {

    public GlobalInvocationTest(ApplicationFixture fixture) : base(fixture) {
    }

    [Fact]
    public void Invocation3() {
        const int Length = 16;

        BufferOutputTestHelper<uint> helper = CreateBufferOutputTestHelper<uint>();

        NeslMethodBuilder main = helper.DefineMethod();
        main.AddAttribute(KernelAttribute.Create(new Vector3<uint>(Length, 1, 1)));
        IlGenerator il = main.IlGenerator;

        il.Emit(OpCode.DefVariable, BuiltInTypes.UInt32);
        il.Emit(OpCode.DefVariable, Vectors.GetVector3(BuiltInTypes.UInt32));
        il.Emit(OpCode.Call, 2u, Compute.GlobalInvocation3, stackalloc uint[0]);
        il.Emit(OpCode.LoadField, 1u, 2u, 0u);
        il.Emit(OpCode.SetElement, 0u, 1u, 1u);
        il.Emit(OpCode.Return);

        // Assert.
        uint[] values = new uint[Length];
        for (uint i = 0; i < Length; i++)
            values[i] = i;

        helper.ExecuteAndAssert(null, values);
    }

    /*[Fact]
    public void Invocation() {
        const int Length = 16;

        BufferOutputTestHelper<uint> helper = CreateBufferOutputTestHelper<uint>();

        NeslMethodBuilder main = helper.DefineMethod();
        main.AddAttribute(KernelAttribute.Create(new Vector3<uint>(Length, 1, 1)));
        IlGenerator il = main.IlGenerator;

        il.Emit(OpCode.DefVariable, BuiltInTypes.UInt32);
        il.Emit(OpCode.Call, 1u, Compute.GlobalInvocation, stackalloc uint[0]);
        il.Emit(OpCode.SetElement, 0u, 1u, 1u);
        il.Emit(OpCode.Return);

        // Assert.
        uint[] values = new uint[Length];
        for (uint i = 0; i < Length; i++)
            values[i] = i;

        helper.ExecuteAndAssert(null, values);
    }*/

}
