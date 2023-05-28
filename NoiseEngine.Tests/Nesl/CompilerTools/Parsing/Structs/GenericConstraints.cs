using NoiseEngine.Nesl;
using NoiseEngine.Tests.Environments;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Parsing.Structs;

public class GenericConstraints : NeslParsingTestEnvironment {

    [Fact]
    public void Single() {
        CompileSingle(@"
            interface Mock {
                f32 Get();
            }

            struct ZeroTwo<T> where T : Mock {
                public f32 Wow(T a) {
                    return a.Get();
                }
            }
        ");
    }

    [Fact]
    public void EmptySingle() {
        CompileSingle(@"
            interface None {}
            struct ZeroTwo<T> where T : None {}
        ");
    }

    [Fact]
    public void Make() {
        CompileSingle(@"
            interface None {}
            struct ZeroTwo<T> where T : None {}
            struct Mock : None {}

            struct Runner {
                void Run(ZeroTwo<Mock> a) {}
            }
        ");
    }

    [Fact]
    public void MakeInvalid() {
        CompileSingleThrow(@"
            interface None {}
            struct ZeroTwo<T> where T : None {}

            struct Runner {
                void Run(ZeroTwo<f32> a) {}
            }
        ", CompilationErrorType.TypeNotSatisfiedGenericConstraint);
    }

}
