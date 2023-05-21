using NoiseEngine.Tests.Environments;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Parsing.Methods;

public class AccessModifiers : NeslParsingTestEnvironment {

    [Fact]
    public void Default() {
        CompileSingle(@"
            struct Kkard2 {
                Mock Method() {
                    return new Mock();
                }
            }

            struct Mock {}
        ");
    }

    [Fact]
    public void Local() {
        CompileSingle(@"
            struct Xori {
                local Mock Method() {
                    return new Mock();
                }
            }

            struct Mock {}
        ");
    }

    [Fact]
    public void Internal() {
        CompileSingle(@"
            struct Vixen {
                internal Mock Method() {
                    return new Mock();
                }
            }

            struct Mock {}
        ");
    }

    [Fact]
    public void Public() {
        CompileSingle(@"
            struct Pp {
                public Mock Method() {
                    return new Mock();
                }
            }

            struct Mock {}
        ");
    }

    [Fact]
    public void NotExists() {
        CompileSingleThrowAny(@"
            struct TerQ {
                NotExistsAccessModifier Mock Method() {
                    return new Mock();
                }
            }

            struct Mock {}
        ");
    }

}
