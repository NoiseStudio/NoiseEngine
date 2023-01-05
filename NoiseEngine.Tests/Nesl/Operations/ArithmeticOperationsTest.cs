using NoiseEngine.Mathematics;
using NoiseEngine.Nesl;
using NoiseEngine.Nesl.Default;
using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.Emit.Attributes;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Environments.Nesl;
using NoiseEngine.Tests.Fixtures;
using System;

namespace NoiseEngine.Tests.Nesl.Operations;

public class ArithmeticOperationsTest : NeslTestEnvironment {

    public ArithmeticOperationsTest(ApplicationFixture fixture) : base(fixture) {
    }

    [Fact]
    public void Negate() {
        ArithmeticHelper(il => il.Emit(OpCode.Negate, 1u, 1u), new object[][] {
            new object[] {
                4.14359f, 6.10783f, -4.64661f, float.NegativeInfinity,
                0f, 0f, 0f, 0f,
                -4.14359f, -6.10783f, 4.64661f, float.PositiveInfinity
            }
        });
    }

    private void ArithmeticHelper(Action<IlGenerator> ilFactory, object[][] values) {
        if (values[0].Length != 0)
            ArithmeticHelperWorkerFloat32(ilFactory, values[0]);
    }

    private void ArithmeticHelperWorkerFloat32(Action<IlGenerator> ilFactory, object[] values) {
        // Scalar.
        BufferOutputTestHelper<float> helperA = CreateBufferOutputTestHelper<float>();
        ArithmeticHelperWorkerMethodFactory(helperA.DefineMethod(), ilFactory, BuiltInTypes.Float32);
        helperA.ExecuteAndAssert(new float[] { (float)values[0], (float)values[4] }, (float)values[8]);

        // Vector4.
        BufferOutputTestHelper<Vector4<float>> helperB = CreateBufferOutputTestHelper<Vector4<float>>();
        ArithmeticHelperWorkerMethodFactory(
            helperB.DefineMethod(), ilFactory, Vectors.GetVector4(BuiltInTypes.Float32)
        );
        helperB.ExecuteAndAssert(
            new Vector4<float>[] {
                new Vector4<float>((float)values[0], (float)values[1], (float)values[2], (float)values[3]),
                new Vector4<float>((float)values[4], (float)values[5], (float)values[6], (float)values[7])
            },
            new Vector4<float>((float)values[8], (float)values[9], (float)values[10], (float)values[11])
        );
    }

    private void ArithmeticHelperWorkerMethodFactory(
        NeslMethodBuilder main, Action<IlGenerator> factory, NeslType elementType
    ) {
        main.AddAttribute(KernelAttribute.Create(Vector3<uint>.One));
        IlGenerator il = main.IlGenerator;

        il.Emit(OpCode.DefVariable, elementType);
        il.Emit(OpCode.DefVariable, elementType);
        il.Emit(OpCode.DefVariable, BuiltInTypes.UInt32);
        il.Emit(OpCode.LoadUInt32, 3u, 1u);
        il.Emit(OpCode.LoadElement, 2u, 0u, 3u);
        il.Emit(OpCode.LoadUInt32, 3u, 0u);
        il.Emit(OpCode.LoadElement, 1u, 0u, 3u);

        factory(il);

        il.Emit(OpCode.SetElement, 0u, 3u, 1u);
        il.Emit(OpCode.Return);
    }

}
