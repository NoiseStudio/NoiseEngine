using NoiseEngine.Mathematics;
using NoiseEngine.Nesl;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Tests.Environments;
using System.Linq;

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

}
