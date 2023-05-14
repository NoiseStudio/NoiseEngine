using NoiseEngine.Tests.Environments;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Parsing.Methods.New;

public class Initialize : NeslParsingTestEnvironment {

    [Fact]
    public void One() {
        CompileSingle(@"
            struct Mock {
                f32 Value;
            }

            Mock Main(f32 a) {
                return new Mock() {
                    Value = a
                };
            }
        ");
    }

    [Fact]
    public void Several() {
        CompileSingle(@"
            struct Mock {
                f32 Value;
                f32v4 Vector;
            }

            Mock Main(f32v4 v, f32 a) {
                return new Mock() {
                    Value = a,
                    Vector = v
                };
            }
        ");
    }

    [Fact]
    public void AdditionalComma() {
        CompileSingleThrowAny(@"
            struct Mock {
                f32 Value;
            }

            Mock Main(f32 a) {
                return new Mock() {
                    Value = a,
                };
            }
        ");
    }

    [Fact]
    public void MissingComma() {
        CompileSingleThrowAny(@"
            struct Mock {
                f32 Value;
                f32v4 Vector;
            }

            Mock Main(f32v4 v, f32 a) {
                return new Mock() {
                    Value = a
                    Vector = v
                };
            }
        ");
    }

}
