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

    [Theory]
    [InlineData(new uint[] {
        32, 51, 44, 91,
        97, 21, 90, 35,
        129, 72, 134, 126
    })]
    [InlineData(new int[] {
        89, -90, 10, 28,
        91, 18, -31, 75,
        180, -72, -21, 103
    })]
    [InlineData(new float[] {
        -3.51834f, 4.60347f, 5.26284f, 4.37281f,
        1.30167f, 6.68611f, -7.74374f, 9.32226f,
        -2.21667f, 11.2895793914794921875f, -2.480900287628173828125f, 13.69507f
    })]
    public void Add(object values) {
        ArithmeticHelper(il => il.Emit(OpCode.Add, 1u, 1u, 2u), values);
    }

    [Theory]
    [InlineData(new uint[] {
        32, 51, 44, 91,
        97, 21, 90, 35,
        4294967231, 30, 4294967250, 56
    })]
    [InlineData(new int[] {
        89, -90, 10, 28,
        91, 18, -31, 75,
        -2, -108, 41, -47
    })]
    [InlineData(new float[] {
        -3.51834f, 64.07903f, 5.26284f, 4.37281f,
        1.30167f, 50.03454f, -7.74374f, 9.32226f,
        -4.82001f, 14.044495f, 13.00658f, -4.94945f
    })]
    public void Substract(object values) {
        ArithmeticHelper(il => il.Emit(OpCode.Subtract, 1u, 1u, 2u), values);
    }

    [Theory]
    [InlineData(new uint[] {
        32, 51, 44, 91,
        97, 21, 90, 35,
        3104, 1071, 3960, 3185
    })]
    [InlineData(new int[] {
        89, -90, 10, 28,
        91, 18, -31, 75,
        8099, -1620, -310, 2100
    })]
    [InlineData(new float[] {
        -3.51834f, 4.60347f, 44.22299f, 67.55519f,
        1.30167f, 6.68611f, -15.03752f, 55.65422f,
        -4.5797176278f, 30.7793068017f, -665.004096585f, 3759.7314064f
    })]
    public void Multiple(object values) {
        ArithmeticHelper(il => il.Emit(OpCode.Multiple, 1u, 1u, 2u), values);
    }

    [Theory]
    [InlineData(new uint[] {
        32, 51, 44, 91,
        97, 21, 90, 35,
        0, 2, 0, 2
    })]
    [InlineData(new int[] {
        89, -90, 10, 28,
        91, 18, -31, 75,
        0, -5, 0, 0
    })]
    [InlineData(new float[] {
        -3.51834f, 4.60347f, 5.26284f, 52.61113f,
        1.30167f, 6.68611f, -7.74374f, 25.45748f,
        -2.702943325042724609375f, 0.6885124534295726513623018466642f, -0.679625034332275390625f,
        2.06662756879f
    })]
    public void Divide(object values) {
        ArithmeticHelper(il => il.Emit(OpCode.Divide, 1u, 1u, 2u), values);
    }

    [Theory]
    [InlineData(new uint[] {
        32, 51, 44, 91,
        97, 21, 90, 35,
        32, 9, 44, 21
    })]
    [InlineData(new int[] {
        89, 90, 18, 28,
        91, 18, 51, 75,
        89, 0, 18, 28
    })]
    [InlineData(new float[] {
        -52.61113f, 4.60347f, 2.29993f, 4.37281f,
        250.45748f, 6.68611f, -7.09268f, 9.32226f,
        197.84635f, 4.60347f, -4.79275f, 4.37281f
    })]
    public void Modulo(object values) {
        ArithmeticHelper(il => il.Emit(OpCode.Modulo, 1u, 1u, 2u), values);
    }

    [Theory]
    [InlineData(new uint[] {
        32, 51, 44, 91,
        97, 21, 90, 35,
        32, 9, 44, 21
    })]
    [InlineData(new int[] {
        89, 13, 10, 28,
        91, 5, -31, 75,
        89, 3, 10, 28
    })]
    [InlineData(new float[] {
        -3.51834f, 4.60347f, 5.26284f, 4.37281f,
        1.30167f, 6.68611f, -7.74374f, 9.32226f,
        -0.9150002f, 4.60347f, 5.26284f, 4.37281f
    })]
    public void Remainder(object values) {
        ArithmeticHelper(il => il.Emit(OpCode.Remainder, 1u, 1u, 2u), values);
    }

    #region Helpers

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
        helperA.ExecuteAndAssert(new T[] { values[0], values[4] }, values[8]);

        // Vector4.
        BufferOutputTestHelper<Vector4<T>> helperB = CreateBufferOutputTestHelper<Vector4<T>>();
        ArithmeticHelperWorkerMethodFactory(helperB.DefineMethod(), ilFactory, Vectors.GetVector4(neslType));
        helperB.ExecuteAndAssert(
            new Vector4<T>[] {
                new Vector4<T>(values[0], values[1], values[2], values[3]),
                new Vector4<T>(values[4], values[5], values[6], values[7])
            },
            new Vector4<T>(values[8], values[9], values[10], values[11])
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

    #endregion
}
