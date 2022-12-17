using NoiseEngine.Mathematics;
using NoiseEngine.Nesl.Default;
using NoiseEngine.Nesl.Emit.Attributes;
using NoiseEngine.Nesl.Emit;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Environments.Nesl;
using NoiseEngine.Tests.Fixtures;
using NoiseEngine.Nesl;
using System;

namespace NoiseEngine.Tests.Nesl.Intrinsics;

public class ReadWriteBufferTest : NeslTestEnvironment {

    public ReadWriteBufferTest(ApplicationFixture fixture) : base(fixture) {
    }

    [Fact]
    public void Indexer() {
        const float Value = 18.64f;
        const uint IndexInitial = 5;
        const uint IndexExpected = 2;

        BufferOutputTestHelper<float> helper = CreateBufferOutputTestHelper<float>();

        NeslMethodBuilder main = helper.DefineMethod();
        main.AddAttribute(KernelAttribute.Create(Vector3<uint>.One));
        IlGenerator il = main.IlGenerator;

        // Get.
        il.Emit(OpCode.DefVariable, BuiltInTypes.UInt32);
        il.Emit(OpCode.LoadUInt32, 1u, IndexInitial);
        il.Emit(OpCode.DefVariable, BuiltInTypes.Float32);
        il.Emit(
            OpCode.Call, 2u, Buffers.GetReadWriteBuffer(BuiltInTypes.Float32).GetMethod(NeslOperators.IndexerGet)!,
            stackalloc uint[] { 0u, 1u }
        );

        // Set.
        il.Emit(OpCode.DefVariable, BuiltInTypes.UInt32);
        il.Emit(OpCode.LoadUInt32, 3u, IndexExpected);
        il.Emit(
            OpCode.Call, uint.MaxValue,
            Buffers.GetReadWriteBuffer(BuiltInTypes.Float32).GetMethod(NeslOperators.IndexerSet)!,
            stackalloc uint[] { 0u, 3u, 2u }
        );

        il.Emit(OpCode.Return);

        // Assert.
        float[] initialValues = new float[Math.Max(IndexInitial, IndexExpected) + 1];
        initialValues[IndexInitial] = Value;

        float[] expectedValues = new float[initialValues.Length];
        expectedValues[IndexInitial] = Value;
        expectedValues[IndexExpected] = Value;

        helper.ExecuteAndAssert(initialValues, expectedValues);
    }

}
