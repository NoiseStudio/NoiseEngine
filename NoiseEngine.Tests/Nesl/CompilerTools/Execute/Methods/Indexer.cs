using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Execute.Methods;

public class Indexer : NeslExecuteTestEnvironment {

    public Indexer(ApplicationFixture fixture) : base(fixture) {
    }

    [Fact]
    public void LoadAndSet() {
        RunKernelWithSingleBuffer(@"
            using System;

            uniform RwBuffer<f32> buffer;

            [Kernel(1, 1, 1)]
            void Main() {
                buffer[0] = buffer[2];
            }
        ", new float[] { -1, 0, 26.05f }, 26.05f);
    }

}
