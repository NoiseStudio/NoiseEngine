using NoiseEngine.Mathematics;
using NoiseEngine.Nesl.Default;
using NoiseEngine.Nesl.Emit.Attributes;
using NoiseEngine.Nesl.Emit;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Environments.Nesl;
using NoiseEngine.Tests.Fixtures;
using NoiseEngine.Nesl;

namespace NoiseEngine.Tests.Nesl.Intrinsics;

public class ReadWriteBufferTest : NeslTestEnvironment {

    public ReadWriteBufferTest(ApplicationFixture fixture) : base(fixture) {
    }

    [Fact]
    public void IndexerGet() {
        const uint Value = 5;

        BufferOutputTestHelper<float> helper = CreateBufferOutputTestHelper<float>();

        NeslMethodBuilder main = helper.DefineMethod();
        main.AddAttribute(KernelAttribute.Create(Vector3<uint>.One));
        IlGenerator il = main.IlGenerator;

        il.Emit(OpCode.DefVariable, BuiltInTypes.UInt32);
        il.Emit(OpCode.LoadUInt32, 1u, Value);
        il.Emit(OpCode.DefVariable, BuiltInTypes.Float32);
        il.Emit(
            OpCode.Call, 2u, Buffers.GetReadWriteBuffer(BuiltInTypes.Float32).GetMethod(NeslOperators.IndexerGet)!,
            stackalloc uint[] { 0u, 1u }
        );
        il.Emit(OpCode.Return);

        helper.ExecuteAndAssert(null, Value);
    }

}
