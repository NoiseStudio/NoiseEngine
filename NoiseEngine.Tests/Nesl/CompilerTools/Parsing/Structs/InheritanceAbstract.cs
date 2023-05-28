using NoiseEngine.Nesl;
using NoiseEngine.Tests.Environments;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Parsing.Structs;

public class InheritanceAbstract : NeslParsingTestEnvironment {

    [Fact]
    public void NotImplement() {
        CompileSingleThrow(@"
            interface IFoo {
                f32 Get();
            }

            struct Bar : IFoo {
            }
        ", CompilationErrorType.AbstractMethodNotImplemented);
    }

    [Fact]
    public void Implement() {
        CompileSingle(@"
            interface IFoo {
                f32 Get();
            }

            struct Bar : IFoo {

                public f32 Get() {
                    return 420.69;
                }

            }
        ");
    }

}
