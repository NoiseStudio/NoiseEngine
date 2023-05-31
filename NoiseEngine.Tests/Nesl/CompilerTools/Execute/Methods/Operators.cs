using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using System;

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

    private void Invoker(object values, string op) {
        if (values is uint[] uintValues)
            InvokerImpl(uintValues, "u32");
        else if (values is int[] intValues)
            InvokerImpl(intValues, "i32");
        else if (values is float[] floatValues)
            InvokerImpl(floatValues, "f32");
        else
            throw new ArgumentException("Invalid type", nameof(values));
    }

    private void InvokerImpl<T>(T[] values, string type) where T : unmanaged {
        for (int i = 1; i <= 1; i++) {
            int block = values.Length / 3;
            int initialLength = block * 2;
            T[] initialValues = values.AsSpan(0, initialLength).ToArray();
            T[] expectedValues = values.AsSpan(initialLength).ToArray();

            RunKernelWithSingleBuffer($@"
                using System;

                uniform RwBuffer<{type}> buffer;

                [Kernel({block}, 1, 1)]
                void Main() {{
                    buffer[ComputeUtils.GlobalInvocation3.X] = {type}.Add(
                        buffer[ComputeUtils.GlobalInvocation3.X], buffer[ComputeUtils.GlobalInvocation3.X]
                    );
                }}
            ", initialValues, expectedValues);
        }
    }

}
