using NoiseEngine.Mathematics;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using System;
using System.Numerics;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Execute.Methods;

public class Operators : NeslExecuteTestEnvironment {

    public Operators(ApplicationFixture fixture) : base(fixture) {
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
        Invoker(values, "+");
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
    public void Subtract(object values) {
        Invoker(values, "-");
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
    public void Multiply(object values) {
        Invoker(values, "*");
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
        Invoker(values, "/");
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
        Invoker(values, "%");
    }

    [Theory]
    [InlineData(new float[] {
        2f, 16f, 7.499995f, 4.2f,
        3f, 0.5f, 4.12f, 2f,
        8f, 4f, 4029.5f, 17.64f
    })]
    public void Power(object values) {
        Invoker(values, "**");
    }

    private void Invoker(object values, string op) {
        // Use typeof instead of is keyword to avoid implicit conversion.
        Type type = values.GetType();
        if (type == typeof(uint[]))
            InvokerImpl((uint[])values, "u32", op);
        else if (type == typeof(ulong[]))
            InvokerImpl((ulong[])values, "u64", op);
        else if (type == typeof(int[]))
            InvokerImpl((int[])values, "i32", op);
        else if (type == typeof(long[]))
            InvokerImpl((long[])values, "i64", op);
        else if (type == typeof(float[]))
            InvokerImpl((float[])values, "f32", op);
        else if (type == typeof(double[]))
            InvokerImpl((double[])values, "f64", op);
        else
            throw new ArgumentException("Invalid type", nameof(values));
    }

    private void InvokerImpl<T>(T[] values, string type, string op) where T : unmanaged, INumber<T> {
        // 1.
        int initialLength = values.Length / 3 * 2;
        T[] initialValues = values.AsSpan(0, initialLength).ToArray();
        T[] expectedValues = values.AsSpan(initialLength).ToArray();
        InvokerImplRun(initialValues, expectedValues, type, op);

        // 2.
        Vector2<T>[] initialValues2 = new Vector2<T>[initialValues.Length / 2];
        for (int i = 0; i < initialValues2.Length; i++)
            initialValues2[i] = new Vector2<T>(initialValues[i * 2], initialValues[i * 2 + 1]);
        Vector2<T>[] expectedValues2 = new Vector2<T>[expectedValues.Length / 2];
        for (int i = 0; i < expectedValues2.Length; i++)
            expectedValues2[i] = new Vector2<T>(expectedValues[i * 2], expectedValues[i * 2 + 1]);
        InvokerImplRun(initialValues2, expectedValues2, $"Vector2<{type}>", op);

        // 3.
        Vector3<T>[] initialValues3 = new Vector3<T>[initialValues.Length / 3];
        for (int i = 0; i < initialValues3.Length; i++) {
            initialValues3[i] = new Vector3<T>(
                initialValues[i * 4], initialValues[i * 4 + 1], initialValues[i * 4 + 2]
            );
        }
        Vector3<T>[] expectedValues3 = new Vector3<T>[expectedValues.Length / 3];
        for (int i = 0; i < expectedValues3.Length; i++) {
            expectedValues3[i] = new Vector3<T>(
                expectedValues[i * 4], expectedValues[i * 4 + 1], expectedValues[i * 4 + 2]
            );
        }
        InvokerImplRun(initialValues3, expectedValues3, $"Vector3<{type}>", op);

        // 4.
        Vector4<T>[] initialValues4 = new Vector4<T>[initialValues.Length / 4];
        for (int i = 0; i < initialValues4.Length; i++) {
            initialValues4[i] = new Vector4<T>(
                initialValues[i * 4], initialValues[i * 4 + 1], initialValues[i * 4 + 2], initialValues[i * 4 + 3]
            );
        }
        Vector4<T>[] expectedValues4 = new Vector4<T>[expectedValues.Length / 4];
        for (int i = 0; i < expectedValues4.Length; i++) {
            expectedValues4[i] = new Vector4<T>(
                expectedValues[i * 4], expectedValues[i * 4 + 1], expectedValues[i * 4 + 2], expectedValues[i * 4 + 3]
            );
        }
        InvokerImplRun(initialValues4, expectedValues4, $"Vector4<{type}>", op);
    }

    private void InvokerImplRun<T>(T[] initialValues, T[] expectedValues, string type, string op) where T : unmanaged {
        RunKernelWithSingleBuffer($@"
            using System;

            uniform RwBuffer<{type}> buffer;

            [Kernel({expectedValues.Length}, 1, 1)]
            void Main() {{
                buffer[ComputeUtils.GlobalInvocation3.X] = buffer[ComputeUtils.GlobalInvocation3.X] {op}
                    buffer[ComputeUtils.GlobalInvocation3.X + {expectedValues.Length}];
            }}
        ", initialValues, expectedValues);
    }

}
