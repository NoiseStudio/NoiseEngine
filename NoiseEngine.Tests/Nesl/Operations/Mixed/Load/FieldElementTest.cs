using NoiseEngine.Mathematics;
using NoiseEngine.Nesl.Default;
using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.Emit.Attributes;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Environments.Nesl;
using NoiseEngine.Tests.Fixtures;
using System;

namespace NoiseEngine.Tests.Nesl.Operations.Mixed.Load;

public class FieldElementTest : NeslTestEnvironment {

    private readonly Random random = new Random();

    public FieldElementTest(ApplicationFixture fixture) : base(fixture) {
    }

    [Fact]
    public void LoadElementLoadFieldSetFieldSetElement() {
        const uint IndexElementInitial = 0;
        const uint IndexElementExpected = 1;
        const uint IndexFieldInitial = 3;
        const uint IndexFieldExpected = 2;

        BufferOutputTestHelper<Vector4<float>> helper = CreateBufferOutputTestHelper<Vector4<float>>();

        NeslMethodBuilder main = helper.DefineMethod();
        main.AddAttribute(KernelAttribute.Create(Vector3<uint>.One));
        IlGenerator il = main.IlGenerator;

        // Get element.
        il.Emit(OpCode.DefVariable, BuiltInTypes.UInt32);
        il.Emit(OpCode.LoadUInt32, 1u, IndexElementInitial);
        il.Emit(OpCode.DefVariable, Vectors.GetVector4(BuiltInTypes.Float32));
        il.Emit(OpCode.LoadElement, 2u, 0u, 1u);

        // Get field.
        il.Emit(OpCode.DefVariable, BuiltInTypes.Float32);
        il.Emit(OpCode.LoadField, 3u, 2u, IndexFieldInitial);

        // Set field.
        il.Emit(OpCode.SetField, 2u, IndexFieldExpected, 3u);

        // Set element.
        il.Emit(OpCode.LoadUInt32, 1u, IndexElementExpected);
        il.Emit(OpCode.SetElement, 0u, 1u, 2u);

        il.Emit(OpCode.Return);

        // Assert.
        Vector4<float>[] initialValues = new Vector4<float>[Math.Max(IndexElementInitial, IndexElementExpected) + 1];
        for (int i = 0; i < initialValues.Length; i++) {
            initialValues[i] = new Vector4<float>(
                random.NextSingle(), random.NextSingle(), random.NextSingle(), random.NextSingle()
            );
        }

        Vector4<float>[] expectedValues = (Vector4<float>[])initialValues.Clone();
        expectedValues[IndexElementExpected] = initialValues[IndexElementInitial] with {
            Z = initialValues[IndexElementInitial].W
        };

        helper.ExecuteAndAssert(initialValues, expectedValues);
    }

}
