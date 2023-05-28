using NoiseEngine.Tests.Environments;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Parsing.Interfaces;

public class Abstract : NeslParsingTestEnvironment {

    [Fact]
    public void MethodWithoutParameters() {
        CompileSingle(@"
            interface HelloWorld {
                f32 Get();
            }
        ");
    }

    [Fact]
    public void MethodWithParameters() {
        CompileSingleThrow(@"
            interface HelloWorld {
                f32 Sum(f32 a, f32 b);
            }
        ");
    }

}
