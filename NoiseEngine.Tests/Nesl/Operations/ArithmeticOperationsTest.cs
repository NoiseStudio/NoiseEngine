using NoiseEngine.Mathematics;
using NoiseEngine.Nesl;
using NoiseEngine.Nesl.Default;
using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.Emit.Attributes;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Environments.Nesl;
using NoiseEngine.Tests.Fixtures;
using System;
using System.Numerics;

namespace NoiseEngine.Tests.Nesl.Operations;

public class ArithmeticOperationsTest : NeslTestEnvironment {

    public ArithmeticOperationsTest(ApplicationFixture fixture) : base(fixture) {
    }

    [Theory]
    [InlineData(new int[] {
        5, 1, -22, -29,
        0, 0, 0, 0,
        -5, -1, 22, 29
    })]
    [InlineData(new float[] {
        4.14359f, 6.10783f, -4.64661f, float.NegativeInfinity,
        0f, 0f, 0f, 0f,
        -4.14359f, -6.10783f, 4.64661f, float.PositiveInfinity
    })]
    public void Negate(object values) {
        ArithmeticHelper(il => il.Emit(OpCode.Negate, 1u, 1u), values);
    }

    /*[Fact]
    public void Add() {
        ArithmeticHelper(il => il.Emit(OpCode.Add, 1u, 1u, 2u), new object[][] {
            new object[] {
                3.51834f, 4.60347f, 5.26284f, 4.37281f,
                1.30167f, 6.68611f, 7.74374f, 9.32226f,
                -4.14359f, -6.10783f, 4.64661f, float.PositiveInfinity
            }
        });
    }*/

    private void ArithmeticHelper(Action<IlGenerator> ilFactory, object values) {
        Type type = values.GetType();

        if (type == typeof(uint[]))
            ArithmeticHelperWorker(ilFactory, (uint[])values);
        else if (type == typeof(int[]))
            ArithmeticHelperWorker(ilFactory, (int[])values);
        else if (type == typeof(float[]))
            ArithmeticHelperWorker(ilFactory, (float[])values);
        else
            throw new InvalidOperationException("Given values type is not supported.");
    }

    private void ArithmeticHelperWorker<T>(
        Action<IlGenerator> ilFactory, T[] values
    ) where T : unmanaged, INumber<T> {
        NeslType neslType = GetNeslTypeFromT<T>();

        // Scalar.
        BufferOutputTestHelper<T> helperA = CreateBufferOutputTestHelper<T>();
        ArithmeticHelperWorkerMethodFactory(helperA.DefineMethod(), ilFactory, neslType);
        helperA.ExecuteAndAssert(new T[] { (T)values[0], (T)values[4] }, (T)values[8]);

        // Vector4.
        BufferOutputTestHelper<Vector4<T>> helperB = CreateBufferOutputTestHelper<Vector4<T>>();
        ArithmeticHelperWorkerMethodFactory(helperB.DefineMethod(), ilFactory, Vectors.GetVector4(neslType));
        helperB.ExecuteAndAssert(
            new Vector4<T>[] {
                new Vector4<T>((T)values[0], (T)values[1], (T)values[2], (T)values[3]),
                new Vector4<T>((T)values[4], (T)values[5], (T)values[6], (T)values[7])
            },
            new Vector4<T>((T)values[8], (T)values[9], (T)values[10], (T)values[11])
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
