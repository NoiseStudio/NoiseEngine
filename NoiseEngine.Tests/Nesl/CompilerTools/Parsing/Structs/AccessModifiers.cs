using NoiseEngine.Tests.Environments;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Parsing.Structs;

public class AccessModifiers : NeslParsingTestEnvironment {

    [Fact]
    public void Default() {
        CompileSingle(@"
            struct Kkard2 {}
        ");
    }

    [Fact]
    public void Local() {
        CompileSingle(@"
            local struct Xori {}
        ");
    }

    [Fact]
    public void Internal() {
        CompileSingle(@"
            internal struct Vixen {}
        ");
    }

    [Fact]
    public void Public() {
        CompileSingle(@"
            public struct Pp {}
        ");
    }

    [Fact]
    public void NotExists() {
        CompileSingleThrowAny(@"
            NotExistsAccessModifier struct TerQ {}
        ");
    }

}
