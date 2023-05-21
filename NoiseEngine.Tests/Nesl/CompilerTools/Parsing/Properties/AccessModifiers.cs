using NoiseEngine.Tests.Environments;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Parsing.Properties;

public class AccessModifiers : NeslParsingTestEnvironment {

    [Fact]
    public void Default() {
        CompileSingle(@"
            struct Kkard2 {
                f32 value { get; }
            }
        ");
    }

    [Fact]
    public void Local() {
        CompileSingle(@"
            struct Xori {
                local f32 value { get; }
            }
        ");
    }

    [Fact]
    public void Internal() {
        CompileSingle(@"
            struct Vixen {
                internal f32 value { get; }
            }
        ");
    }

    [Fact]
    public void Public() {
        CompileSingle(@"
            struct Pp {
                public f32 Value { get; }
            }
        ");
    }

    [Fact]
    public void NotExists() {
        CompileSingleThrowAny(@"
            struct TerQ {
                NotExistsAccessModifier f32 value { get; }
            }
        ");
    }

}
