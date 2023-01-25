using NoiseEngine.Mathematics;
using NoiseEngine.Nesl.Default;
using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.Emit.Attributes;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Environments.Nesl;
using NoiseEngine.Tests.Fixtures;
using System;
using System.Buffers.Binary;

namespace NoiseEngine.Tests.Nesl.Operations;

public class DefaultDataTest : NeslTestEnvironment {

    public DefaultDataTest(ApplicationFixture fixture) : base(fixture) {
    }

    [Fact]
    public void Vector4Float() {
        const int Length = 16;

        Vector4<float>[] vectors = new Vector4<float>[Length];
        for (int i = 0; i < vectors.Length; i++) {
            vectors[i] = new Vector4<float>(
                Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle(),
                Random.Shared.NextSingle()
            );
        }

        BufferOutputTestHelper<Vector4<float>> helper = CreateBufferOutputTestHelper<Vector4<float>>();

        NeslFieldBuilder field = helper.DefineField(
            Buffers.GetReadWriteBuffer(Vectors.GetVector4(BuiltInTypes.Float32))
        );
        byte[] data = new byte[4 * sizeof(float) * vectors.Length];

        int index = 0;
        foreach (Vector4<float> vector in vectors) {
            BinaryPrimitives.WriteSingleLittleEndian(data.AsSpan(index), vector.X);
            index += sizeof(float);
            BinaryPrimitives.WriteSingleLittleEndian(data.AsSpan(index), vector.Y);
            index += sizeof(float);
            BinaryPrimitives.WriteSingleLittleEndian(data.AsSpan(index), vector.Z);
            index += sizeof(float);
            BinaryPrimitives.WriteSingleLittleEndian(data.AsSpan(index), vector.W);
            index += sizeof(float);
        }

        field.SetDefaultData(data);

        NeslMethodBuilder main = helper.DefineMethod();
        main.AddAttribute(KernelAttribute.Create(new Vector3<uint>(Length, 1, 1)));
        IlGenerator il = main.IlGenerator;

        il.Emit(OpCode.DefVariable, BuiltInTypes.UInt32);
        il.Emit(OpCode.DefVariable, Vectors.GetVector3(BuiltInTypes.UInt32));
        il.Emit(OpCode.Call, 3u, Compute.GlobalInvocation3, stackalloc uint[0]);
        il.Emit(OpCode.LoadField, 2u, 3u, 0u);
        il.Emit(OpCode.DefVariable, Vectors.GetVector4(BuiltInTypes.Float32));
        il.Emit(OpCode.LoadElement, 4u, 1u, 2u);
        il.Emit(OpCode.SetElement, 0u, 2u, 4u);
        il.Emit(OpCode.Return);

        // Assert.
        uint[] values = new uint[Length];
        for (uint i = 0; i < Length; i++)
            values[i] = i;

        helper.ExecuteAndAssert(null, vectors);
    }

}
