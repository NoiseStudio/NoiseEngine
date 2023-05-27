using NoiseEngine.Tests.Environments;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Parsing.Methods;

public class Indexer : NeslParsingTestEnvironment {

    [Fact]
    public void SetConst() {
        CompileSingle(@"
            using System;

            uniform RwBuffer<f32> buffer;

            [Kernel(1, 1, 1)]
            void Main() {
                buffer[0] = 11.04;
            }
        ");
    }

    [Fact]
    public void LoadAndSet() {
        CompileSingle(@"
            using System;

            uniform RwBuffer<f32> buffer;

            [Kernel(1, 1, 1)]
            void Main() {
                buffer[7] = buffer[3];
            }
        ");
    }

}
