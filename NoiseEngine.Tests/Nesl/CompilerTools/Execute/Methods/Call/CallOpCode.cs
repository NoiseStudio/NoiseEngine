using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Execute.Methods.Call;

public class CallOpCode : NeslExecuteTestEnvironment {

    public CallOpCode(ApplicationFixture fixture) : base(fixture) {
    }

    [Fact]
    public void Add() {
        RunKernelWithSingleBuffer(@"
            using System;

            uniform RwBuffer<f32> buffer;

            [Kernel(1, 1, 1)]
            void Main() {
                buffer[0] = f32.Add(1.0, 2.0);
            }
        ", null, 3f);
    }

}
